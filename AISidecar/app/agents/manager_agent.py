import json
import time
from functools import lru_cache
from typing import Annotated, TypedDict

from langchain_core.messages import AIMessage, HumanMessage, ToolMessage
from langchain_core.runnables import RunnableConfig
from langgraph.checkpoint.memory import MemorySaver
from langgraph.config import get_stream_writer
from langgraph.graph import END, StateGraph
from langgraph.graph.message import add_messages
from pydantic import ValidationError

from app.core.errors import ForbiddenError
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
from app.services.chat_tools import build_tools, describe_args
from app.services.routing import resolve_modules
from app.tools.registry import build_tool_scope

STEERING_POLL_INTERVAL_SECONDS = 0.7
MAX_TOOL_TURNS = 8

GUARDRAIL_STATE_KEY = "permissions"


class AgentState(TypedDict):
    messages: Annotated[list, add_messages]
    run_id: str
    auth_header: str
    turns: int
    absorbed_count: int
    carried_steering: list
    cancelled: bool
    tool_turns: int
    tool_limit_reached: bool
    permissions: list[str]
    allowed_tool_names: set[str]
    tool_budget: int
    tool_call_count: int
    call_signatures: set[str]
    had_forbidden_tool: bool
    plan_approved: bool
    history: list[dict]
    routing_context: dict
    scoped_modules: list[str]
    expanded_modules: set[str]
    current_plan_step: dict | None


def _latest_human_text(messages) -> str:
    for msg in reversed(messages or []):
        if isinstance(msg, HumanMessage):
            return msg.content
    return ""


def build_steering_message(item: dict) -> HumanMessage:
    if item["mode"] == "interrupt":
        return HumanMessage(content=(
            f"[ĐÍNH CHÍNH TỪ NGƯỜI DÙNG] {item['content']}\n"
            "Hãy điều chỉnh theo thông tin mới này. "
            "Bỏ qua phần công việc đã làm nếu không còn phù hợp, "
            "và KHÔNG trả lời cho yêu cầu cũ nữa."
        ))
    return HumanMessage(content=f"[BỔ SUNG TỪ NGƯỜI DÙNG] {item['content']}")


async def absorb_steering_node(state: AgentState) -> dict:
    carried = state.get("carried_steering") or []
    if carried:
        pending = carried
    else:
        client = BackendClient(state["auth_header"])
        pending = await client.pull_pending_steering(state["run_id"])

    scoping = GUARDRAIL_STATE_KEY in state
    routing_ctx = state.get("routing_context") or {}
    history = state.get("history") or []
    updates = {}

    if not pending:
        if scoping and state.get("scoped_modules") is None:
            query = _latest_human_text(state.get("messages"))
            updates["scoped_modules"] = await resolve_modules(query, routing_ctx, history)
            updates["expanded_modules"] = set()
        return {"absorbed_count": 0, "carried_steering": [], **updates}

    writer = get_stream_writer()
    writer(("turn_boundary", ""))
    new_messages = [build_steering_message(item) for item in pending]
    modes = {item["mode"] for item in pending}
    for item in pending:
        if item["mode"] == "interrupt":
            writer(("run_redirected", "user_correction"))

    if scoping:
        if "interrupt" in modes:
            updates["scoped_modules"] = await resolve_modules(pending[-1]["content"], routing_ctx, history)
            updates["expanded_modules"] = set()
        else:
            extra = await resolve_modules(pending[-1]["content"], routing_ctx, history)
            current = state.get("scoped_modules") or []
            updates["scoped_modules"] = list(dict.fromkeys([*current, *extra]))[:3]

    return {"messages": new_messages, "absorbed_count": len(pending), "carried_steering": [], **updates}


async def call_model_node(state: AgentState, config: RunnableConfig) -> dict:
    writer = get_stream_writer()
    client = BackendClient(state["auth_header"])
    llm = get_llm(temperature=0.3)
    updates = {}

    all_tools = build_tools(client)
    scoping = GUARDRAIL_STATE_KEY in state
    if scoping:
        allowed_names = {spec.name for spec in build_tool_scope(state)}
        tools = [t for t in all_tools if t.name in allowed_names]
        updates["allowed_tool_names"] = allowed_names
    else:
        tools = all_tools

    if hasattr(llm, "bind_tools"):
        llm = llm.bind_tools(tools)
    cancel_event = config.get("configurable", {}).get("cancel_event")

    full = None
    carried = []
    last_poll = time.monotonic()

    async for chunk in llm.astream(state["messages"]):
        if cancel_event is not None and cancel_event.is_set():
            return {"turns": state.get("turns", 0) + 1, "cancelled": True, "carried_steering": []}

        content = chunk if isinstance(chunk, str) else (getattr(chunk, "content", "") or "")
        if content:
            writer(("text_delta", content))

        full = chunk if full is None else full + chunk

        now = time.monotonic()
        if now - last_poll >= STEERING_POLL_INTERVAL_SECONDS:
            last_poll = now
            pulled = await client.pull_pending_steering(state["run_id"])
            if pulled:
                carried.extend(pulled)
                if any(item["mode"] == "interrupt" for item in pulled):
                    return {"turns": state.get("turns", 0) + 1, "carried_steering": carried}

    if full is None:
        result_message = AIMessage(content="")
    elif isinstance(full, str):
        result_message = AIMessage(content=full)
    else:
        result_message = full

    if scoping and not getattr(result_message, "tool_calls", None):
        guard = check_output(result_message.content or "", {
            "had_forbidden_tool": state.get("had_forbidden_tool", False),
            "tool_call_count": state.get("tool_call_count", 0),
        })
        if guard.action == "block":
            result_message = AIMessage(content="Không thể trả lời yêu cầu này.")
            writer(("message_correction", result_message.content))
        elif guard.action == "rewrite":
            result_message = AIMessage(content=(
                "Tôi không có đủ quyền hoặc công cụ để tra dữ liệu này. "
                "Vui lòng liên hệ quản trị viên nếu bạn cần được cấp thêm quyền."
            ))
            writer(("message_correction", result_message.content))

    return {
        "turns": state.get("turns", 0) + 1,
        "carried_steering": carried,
        "messages": [result_message],
        **updates,
    }


async def call_tools_node(state: AgentState) -> dict:
    writer = get_stream_writer()
    last_message = state["messages"][-1]
    tool_calls = getattr(last_message, "tool_calls", None) or []
    tool_turns = state.get("tool_turns", 0) + 1

    if tool_turns > MAX_TOOL_TURNS:
        limit_message = AIMessage(content=(
            "Đã đạt giới hạn truy vấn dữ liệu cho câu hỏi này, "
            "vui lòng hỏi cụ thể hơn."
        ))
        return {"tool_turns": tool_turns, "tool_limit_reached": True, "messages": [limit_message]}

    client = BackendClient(state["auth_header"])
    tools_by_name = {tool.name: tool for tool in build_tools(client)}

    guarding = "allowed_tool_names" in state
    call_signatures = set(state.get("call_signatures") or set())
    tool_call_count = state.get("tool_call_count", 0)
    had_forbidden_tool = state.get("had_forbidden_tool", False)

    result_messages = []
    for tool_call in tool_calls:
        name = tool_call["name"]
        args = dict(tool_call["args"])
        summary = describe_args(name, args)
        writer(("tool_start", json.dumps({"name": name, "summary": summary}, ensure_ascii=False)))

        if guarding:
            guard = check_tool_call(name, args, {
                "allowed_tool_names": state["allowed_tool_names"],
                "tool_call_count": tool_call_count,
                "tool_budget": state.get("tool_budget", DEFAULT_TOOL_BUDGET),
                "call_signatures": call_signatures,
                "is_write": False,
                "plan_approved": state.get("plan_approved", False),
            })
            if guard.action != "allow":
                result_messages.append(ToolMessage(
                    content=json.dumps({"error": guard.message}, ensure_ascii=False),
                    tool_call_id=tool_call["id"],
                ))
                writer(("tool_end", json.dumps({"name": name}, ensure_ascii=False)))
                writer(("guardrail_blocked", json.dumps(
                    {"tool": name, "reason": guard.message}, ensure_ascii=False)))
                continue
            args = guard.args
            call_signatures.add(call_signature(name, args))
            tool_call_count += 1

        tool = tools_by_name.get(name)
        try:
            if tool is None:
                raise ValueError(f"Unknown tool: {name}")
            result = await tool.ainvoke(args)
            if guarding:
                result, flagged = sanitize_tool_result(result)
                if flagged:
                    writer(("guardrail_blocked", json.dumps(
                        {"tool": name, "reason": "injection_detected"}, ensure_ascii=False)))
            content = result if isinstance(result, str) else json.dumps(result, ensure_ascii=False)
            if guarding:
                content = wrap_tool_result(name, content)
        except ForbiddenError:
            had_forbidden_tool = True
            content = json.dumps({"error": "Bạn không có quyền truy cập dữ liệu này."}, ensure_ascii=False)
        except ValidationError as exc:
            bad_fields = ", ".join(e["loc"][0] for e in exc.errors() if e["loc"])
            content = json.dumps({
                "error": f"Tham số truyền vào tool '{name}' sai kiểu dữ liệu ở field: {bad_fields}. "
                         "Mỗi field phải là giá trị đơn giản (chuỗi/số), KHÔNG lồng object. Hãy gọi lại tool này "
                         "với đúng kiểu dữ liệu như mô tả tool.",
            }, ensure_ascii=False)
        except Exception as exc:
            content = json.dumps({"error": str(exc)}, ensure_ascii=False)
        result_messages.append(ToolMessage(content=content, tool_call_id=tool_call["id"]))
        writer(("tool_end", json.dumps({"name": name}, ensure_ascii=False)))

    updates = {"tool_turns": tool_turns, "tool_limit_reached": False, "messages": result_messages}
    if guarding:
        updates["call_signatures"] = call_signatures
        updates["tool_call_count"] = tool_call_count
        updates["had_forbidden_tool"] = had_forbidden_tool
    return updates


def route_after_absorb(state: AgentState) -> str:
    if state.get("cancelled"):
        return "end"
    if state.get("turns", 0) == 0:
        return "continue"
    if state.get("absorbed_count", 0) > 0:
        return "continue"
    return "end"


def route_after_model(state: AgentState) -> str:
    last_message = state["messages"][-1]
    if getattr(last_message, "tool_calls", None):
        return "call_tools"
    return "absorb_steering"


def route_after_tools(state: AgentState) -> str:
    if state.get("tool_limit_reached"):
        return "absorb_steering"
    return "call_model"


def build_graph():
    graph = StateGraph(AgentState)
    graph.add_node("absorb_steering", absorb_steering_node)
    graph.add_node("call_model", call_model_node)
    graph.add_node("call_tools", call_tools_node)
    graph.set_entry_point("absorb_steering")
    graph.add_conditional_edges("absorb_steering", route_after_absorb, {
        "continue": "call_model",
        "end": END,
    })
    graph.add_conditional_edges("call_model", route_after_model, {
        "call_tools": "call_tools",
        "absorb_steering": "absorb_steering",
    })
    graph.add_conditional_edges("call_tools", route_after_tools, {
        "call_model": "call_model",
        "absorb_steering": "absorb_steering",
    })
    return graph.compile(checkpointer=MemorySaver())


@lru_cache
def get_graph():
    return build_graph()
