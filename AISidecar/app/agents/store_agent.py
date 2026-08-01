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
    check_tool_call,
    sanitize_tool_result,
    wrap_tool_result,
)
from app.services.backend_client import BackendClient
from app.services.store_tools import STORE_TOOL_NAMES, build_store_tools

logger = logging.getLogger(__name__)

MAX_TOOL_TURNS = 6


class StoreAgentState(TypedDict):
    messages: Annotated[list, add_messages]
    tool_turns: int
    tool_limit_reached: bool
    tool_call_count: int
    call_signatures: set[str]


def _latest_human_text(messages) -> str:
    for msg in reversed(messages or []):
        if isinstance(msg, HumanMessage):
            return msg.content
    return ""


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
                "colorName": variant.get("variantName"),
                "sku": variant.get("sku"),
                "price": variant.get("price"),
                "slug": variant.get("slug"),
            }
            for variant in (product.get("variants") or [])
        ]
        writer(("variant-cards", json.dumps(
            {"productId": product.get("productId"), "items": variant_cards}, ensure_ascii=False)))


async def call_model_node(state: StoreAgentState) -> dict:
    writer = get_stream_writer()
    tools = build_store_tools(BackendClient(""))
    llm = get_llm(temperature=0.3)
    if hasattr(llm, "bind_tools"):
        llm = llm.bind_tools(tools)

    full = None
    async for chunk in llm.astream(state["messages"]):
        content = chunk if isinstance(chunk, str) else (getattr(chunk, "content", "") or "")
        if content:
            writer(("text_delta", content))
        full = chunk if full is None else full + chunk

    if full is None:
        result_message = AIMessage(content="")
    elif isinstance(full, str):
        result_message = AIMessage(content=full)
    else:
        result_message = full

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
    tools_by_name = {tool.name: tool for tool in build_store_tools(client)}
    call_signatures = set(state.get("call_signatures") or set())
    tool_call_count = state.get("tool_call_count", 0)

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
            "is_write": False,
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
    }


def route_after_model(state: StoreAgentState) -> str:
    last_message = state["messages"][-1]
    if getattr(last_message, "tool_calls", None):
        return "call_tools"
    return "end"


def route_after_tools(state: StoreAgentState) -> str:
    if state.get("tool_limit_reached"):
        return "end"
    return "call_model"


def build_store_graph():
    graph = StateGraph(StoreAgentState)
    graph.add_node("call_model", call_model_node)
    graph.add_node("call_tools", call_tools_node)
    graph.set_entry_point("call_model")
    graph.add_conditional_edges("call_model", route_after_model, {"call_tools": "call_tools", "end": END})
    graph.add_conditional_edges("call_tools", route_after_tools, {"call_model": "call_model", "end": END})
    return graph.compile(checkpointer=MemorySaver())


@lru_cache
def get_store_graph():
    return build_store_graph()
