import json
import logging
from functools import lru_cache
from typing import Annotated, TypedDict

from langchain_core.messages import AIMessage, HumanMessage, ToolMessage
from langgraph.checkpoint.memory import MemorySaver
from langgraph.config import get_stream_writer
from langgraph.graph import END, StateGraph
from langgraph.graph.message import add_messages
from pydantic import ValidationError

from app.core.llm import get_llm
from app.guardrails.tool_guard import (
    DEFAULT_TOOL_BUDGET,
    call_signature,
    check_output,
    check_tool_call,
    sanitize_tool_result,
    wrap_tool_result,
)
from app.services.backend_client import BackendClient
from app.services.store_tools import IS_WRITE_BY_NAME, STORE_TOOL_NAMES, build_store_tools

logger = logging.getLogger(__name__)

MAX_TOOL_TURNS = 6

STORE_REWRITE_MESSAGES = {
    "no_permission": "Xin lỗi, tôi chưa thể trả lời câu hỏi này. Bạn vui lòng bấm \"Gặp nhân viên\" để được hỗ trợ nhé.",
    "unverified_metric": "Tôi chưa tra cứu được thông tin này, bạn cho tôi biết cụ thể hơn (ví dụ tên xe) để tôi tìm chính xác nhé.",
    "stalled_promise": "Xin lỗi, để tôi tra cứu ngay. Bạn nhắc lại yêu cầu hoặc cho thêm chi tiết giúp tôi không?",
}


class StoreAgentState(TypedDict):
    messages: Annotated[list, add_messages]
    session_id: str
    tool_turns: int
    tool_limit_reached: bool
    tool_call_count: int
    call_signatures: set[str]
    recommendation_nudge_used: bool
    escalated: bool


def _latest_human_text(messages) -> str:
    for msg in reversed(messages or []):
        if isinstance(msg, HumanMessage):
            return msg.text
    return ""


_ADVICE_KEYWORDS = ("gợi ý", "tư vấn", "nên mua", "nên chọn", "giới thiệu", "đề xuất")


def _looks_like_vehicle_advice_request(text: str) -> bool:
    lowered = (text or "").lower()
    return "xe" in lowered and any(keyword in lowered for keyword in _ADVICE_KEYWORDS)


_ESCALATION_CLAIM_MARKERS = (
    "chuyển đến nhân viên", "chuyển cho nhân viên", "chuyển sang nhân viên", "chuyển bạn đến",
    "chuyển bạn cho", "chuyển bạn sang", "chuyển phiên", "nhân viên sẽ tiếp nhận",
    "nhân viên sẽ liên hệ", "nhân viên sẽ hỗ trợ", "đội ngũ nhân viên", "được chuyển đến",
    "sẽ được chuyển", "phiên chat này sẽ kết thúc", "nhân viên thật",
)


def _claims_escalation_without_tool(text: str) -> bool:
    lowered = (text or "").lower()
    return any(marker in lowered for marker in _ESCALATION_CLAIM_MARKERS)


def _last_message_is_escalation_nudge(messages) -> bool:
    if not messages:
        return False
    last = messages[-1]
    content = getattr(last, "text", "") or ""
    return isinstance(last, HumanMessage) and content.startswith("[Hệ thống]") and "escalate_to_staff" in content


def _emit_cards(writer, name: str, result) -> None:
    if not isinstance(result, dict):
        return
    items = result.get("items") or []
    if name == "search_products" and items:
        cards = [
            {
                "productId": item.get("productId"),
                "name": item.get("productName"),
                "imageUrl": item.get("imageUrl"),
                "priceFrom": item.get("priceFrom"),
                "priceTo": item.get("priceTo"),
            }
            for item in items
        ]
        writer(("product-cards", json.dumps({"items": cards}, ensure_ascii=False)))
    elif name == "get_product_detail" and items:
        product = items[0]
        variant_cards = [
            {
                "variantId": variant.get("variantId"),
                "variantName": variant.get("variantName"),
                "productName": product.get("productName"),
                "sku": variant.get("sku"),
                "price": variant.get("price"),
                "slug": variant.get("slug"),
                "colors": [
                    {
                        "colorId": color.get("colorId"),
                        "colorName": color.get("colorName"),
                        "colorCode": color.get("colorCode"),
                        "imageUrl": color.get("imageUrl"),
                    }
                    for color in (variant.get("colors") or [])
                ],
            }
            for variant in (product.get("variants") or [])
        ]
        writer(("variant-cards", json.dumps(
            {"productId": product.get("productId"), "items": variant_cards}, ensure_ascii=False)))


async def call_model_node(state: StoreAgentState) -> dict:
    writer = get_stream_writer()
    tools = build_store_tools(BackendClient(""), state.get("session_id", ""))
    llm = get_llm(temperature=0.3)
    if hasattr(llm, "bind_tools"):
        llm = llm.bind_tools(tools)

    is_risky_turn = (
        state.get("tool_call_count", 0) == 0
        and not state.get("recommendation_nudge_used")
        and _looks_like_vehicle_advice_request(_latest_human_text(state["messages"]))
    )

    full = None
    async for chunk in llm.astream(state["messages"]):
        content = chunk if isinstance(chunk, str) else (getattr(chunk, "text", "") or "")
        if content and not is_risky_turn:
            writer(("text_delta", content))
        full = chunk if full is None else full + chunk

    if full is None:
        result_message = AIMessage(content="")
    elif isinstance(full, str):
        result_message = AIMessage(content=full)
    else:
        result_message = full
        if not isinstance(result_message.content, str):
            result_message.content = result_message.text

    tool_calls = getattr(result_message, "tool_calls", None) or []
    if any(call.get("name") == "escalate_to_staff" for call in tool_calls):
        if result_message.content:
            writer(("message_correction", ""))
        result_message = AIMessage(content="", tool_calls=tool_calls)

    if is_risky_turn and not getattr(result_message, "tool_calls", None):
        nudge = HumanMessage(content=(
            "[Hệ thống] Bạn chưa gọi tool nào để tra dữ liệu thật trước khi trả lời câu hỏi tư vấn/mua "
            "xe vừa rồi. Hãy gọi tool search_products (từ khoá phù hợp với nhu cầu khách vừa nêu, hoặc "
            "để trống \"\" nếu chưa đủ thông tin lọc) NGAY BÂY GIỜ trước khi trả lời tiếp — KHÔNG tự nêu "
            "tên xe nào khi chưa có kết quả tool."
        ))
        writer(("guardrail_blocked", json.dumps(
            {"tool": "", "reason": "vehicle_recommendation_without_tool_call"}, ensure_ascii=False)))
        return {"messages": [nudge], "recommendation_nudge_used": True}

    no_tool_call = not getattr(result_message, "tool_calls", None)
    if no_tool_call and _last_message_is_escalation_nudge(state["messages"]):
        if result_message.content:
            writer(("message_correction", ""))
        writer(("guardrail_blocked", json.dumps(
            {"tool": "escalate_to_staff", "reason": "no_tool_call_after_escalation_nudge_forcing_tool_call"},
            ensure_ascii=False)))
        forced_call = AIMessage(content="", tool_calls=[
            {"name": "escalate_to_staff", "args": {}, "id": "forced-escalate-to-staff", "type": "tool_call"},
        ])
        return {"messages": [forced_call]}

    if no_tool_call and _claims_escalation_without_tool(result_message.content or ""):
        if result_message.content:
            writer(("message_correction", ""))
        nudge = HumanMessage(content=(
            "[Hệ thống] Bạn vừa nói sẽ/đã chuyển khách sang nhân viên nhưng CHƯA gọi tool "
            "escalate_to_staff trong lượt này. Hãy gọi tool escalate_to_staff NGAY BÂY GIỜ — nếu không "
            "gọi tool, phiên sẽ KHÔNG được chuyển dù bạn có nói vậy với khách."
        ))
        writer(("guardrail_blocked", json.dumps(
            {"tool": "escalate_to_staff", "reason": "escalation_claimed_without_tool_call"}, ensure_ascii=False)))
        return {"messages": [nudge]}

    if not getattr(result_message, "tool_calls", None):
        guard = check_output(result_message.content or "", {
            "had_forbidden_tool": False,
            "tool_call_count": state.get("tool_call_count", 0),
            "has_tools_bound": bool(tools),
        })
        if guard.action == "block":
            result_message = AIMessage(content="Không thể trả lời yêu cầu này.")
            writer(("message_correction", result_message.content))
        elif guard.action == "rewrite":
            result_message = AIMessage(
                content=STORE_REWRITE_MESSAGES.get(guard.kind, STORE_REWRITE_MESSAGES["no_permission"]))
            writer(("message_correction", result_message.content))

    return {"messages": [result_message]}


async def call_tools_node(state: StoreAgentState) -> dict:
    writer = get_stream_writer()
    last_message = state["messages"][-1]
    tool_calls = getattr(last_message, "tool_calls", None) or []
    tool_turns = state.get("tool_turns", 0) + 1

    if tool_turns > MAX_TOOL_TURNS:
        limit_message = AIMessage(content="Đã đạt giới hạn tra cứu cho câu hỏi này, vui lòng hỏi cụ thể hơn.")
        return {"tool_turns": tool_turns, "tool_limit_reached": True, "messages": [limit_message]}

    client = BackendClient("")
    tools_by_name = {tool.name: tool for tool in build_store_tools(client, state.get("session_id", ""))}
    call_signatures = set(state.get("call_signatures") or set())
    tool_call_count = state.get("tool_call_count", 0)
    escalated = state.get("escalated", False)

    result_messages = []
    for tool_call in tool_calls:
        name = tool_call["name"]
        args = dict(tool_call["args"])
        writer(("tool_start", json.dumps({"name": name}, ensure_ascii=False)))

        if name not in STORE_TOOL_NAMES:
            message = f"Tool '{name}' không nằm trong phạm vi hỗ trợ của trợ lý Store."
            result_messages.append(ToolMessage(
                content=json.dumps({"error": message}, ensure_ascii=False), tool_call_id=tool_call["id"]))
            writer(("tool_end", json.dumps({"name": name}, ensure_ascii=False)))
            writer(("guardrail_blocked", json.dumps({"tool": name, "reason": "tool_not_in_store_scope"}, ensure_ascii=False)))
            continue

        guard = check_tool_call(name, args, {
            "allowed_tool_names": STORE_TOOL_NAMES,
            "tool_call_count": tool_call_count,
            "tool_budget": DEFAULT_TOOL_BUDGET,
            "call_signatures": call_signatures,
            "is_write": IS_WRITE_BY_NAME.get(name, False),
            "plan_approved": True,
        })
        if guard.action != "allow":
            result_messages.append(ToolMessage(
                content=json.dumps({"error": guard.message}, ensure_ascii=False), tool_call_id=tool_call["id"]))
            writer(("tool_end", json.dumps({"name": name}, ensure_ascii=False)))
            writer(("guardrail_blocked", json.dumps({"tool": name, "reason": guard.message}, ensure_ascii=False)))
            continue

        args = guard.args
        call_signatures.add(call_signature(name, args))
        tool_call_count += 1

        tool = tools_by_name[name]
        try:
            result = await tool.ainvoke(args)
            result, flagged = sanitize_tool_result(result)
            if flagged:
                writer(("guardrail_blocked", json.dumps({"tool": name, "reason": "injection_detected"}, ensure_ascii=False)))
            _emit_cards(writer, name, result)
            content = wrap_tool_result(name, json.dumps(result, ensure_ascii=False))
            if name == "escalate_to_staff" and isinstance(result, dict):
                items = result.get("items") or []
                if items and isinstance(items[0], dict) and items[0].get("escalated"):
                    escalated = True
        except ValidationError as exc:
            bad_fields = ", ".join(e["loc"][0] for e in exc.errors() if e["loc"])
            content = json.dumps({
                "error": f"Tham số truyền vào tool '{name}' sai kiểu dữ liệu ở field: {bad_fields}. "
                         "Hãy gọi lại tool này với đúng kiểu dữ liệu như mô tả.",
            }, ensure_ascii=False)
        except Exception as exc:
            content = json.dumps({"error": str(exc)}, ensure_ascii=False)
        result_messages.append(ToolMessage(content=content, tool_call_id=tool_call["id"]))
        writer(("tool_end", json.dumps({"name": name}, ensure_ascii=False)))

    return {
        "tool_turns": tool_turns,
        "tool_limit_reached": False,
        "messages": result_messages,
        "call_signatures": call_signatures,
        "tool_call_count": tool_call_count,
        "escalated": escalated,
    }


def route_after_model(state: StoreAgentState) -> str:
    last_message = state["messages"][-1]
    if isinstance(last_message, HumanMessage):
        return "call_model"
    if getattr(last_message, "tool_calls", None):
        return "call_tools"
    return "end"


def route_after_tools(state: StoreAgentState) -> str:
    if state.get("tool_limit_reached"):
        return "end"
    if state.get("escalated"):
        return "end"
    return "call_model"


def build_store_graph():
    graph = StateGraph(StoreAgentState)
    graph.add_node("call_model", call_model_node)
    graph.add_node("call_tools", call_tools_node)
    graph.set_entry_point("call_model")
    graph.add_conditional_edges(
        "call_model", route_after_model, {"call_model": "call_model", "call_tools": "call_tools", "end": END})
    graph.add_conditional_edges("call_tools", route_after_tools, {"call_model": "call_model", "end": END})
    return graph.compile(checkpointer=MemorySaver())


@lru_cache
def get_store_graph():
    return build_store_graph()
