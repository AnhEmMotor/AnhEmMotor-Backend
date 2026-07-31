import json
import logging
import re
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
from app.core.redaction import _scrub_text, make_tool_preview
from app.guardrails.tool_guard import (
    DEFAULT_TOOL_BUDGET,
    call_signature,
    check_known_id,
    check_output,
    check_tool_call,
    extract_produced_ids,
    sanitize_tool_result,
    wrap_tool_result,
)
from app.services.backend_client import BackendClient
from app.services.chat_tools import build_tools, describe_args, summarize_result
from app.services.prompt_builder import build_plan_prompt
from app.services.routing import resolve_modules
from app.tools.registry import build_tool_scope, load_tool_specs, registry_fingerprint, resolve_tool_call_error

logger = logging.getLogger(__name__)

STEERING_POLL_INTERVAL_SECONDS = 0.7
MAX_TOOL_TURNS = 8
MAX_PLAN_STEPS = 8

PLAN_MODE_KEYWORDS = ("báo cáo", "phân tích", "tổng hợp", "so sánh", "lập kế hoạch", "kiểm tra toàn bộ")

GUARDRAIL_STATE_KEY = "permissions"

THINKING_OPEN = "<suy_nghi>"
THINKING_CLOSE = "</suy_nghi>"


class ThinkingParser:
    def __init__(self):
        self._buffer = ""
        self._resolved = False
        self._inside = False
        self.thinking_text = ""
        self.tag_closed = False

    def feed(self, content: str) -> str:
        if self._resolved and not self._inside:
            return content
        self._buffer += content
        if not self._resolved:
            if len(self._buffer) < len(THINKING_OPEN):
                if THINKING_OPEN.startswith(self._buffer):
                    return ""
                self._resolved = True
                out, self._buffer = self._buffer, ""
                return out
            self._resolved = True
            if self._buffer.startswith(THINKING_OPEN):
                self._inside = True
                self._buffer = self._buffer[len(THINKING_OPEN):]
            else:
                out, self._buffer = self._buffer, ""
                return out
        if self._inside:
            idx = self._buffer.find(THINKING_CLOSE)
            if idx == -1:
                return ""
            self.thinking_text = self._buffer[:idx]
            self.tag_closed = True
            rest = self._buffer[idx + len(THINKING_CLOSE):]
            self._inside, self._buffer = False, ""
            return rest
        return ""


REWRITE_MESSAGES = {
    "no_permission": (
        "Tôi không có đủ quyền để tra dữ liệu này. "
        "Vui lòng liên hệ quản trị viên nếu bạn cần được cấp thêm quyền."
    ),
    "unverified_metric": (
        "Tôi chưa tra cứu được dữ liệu thật cho câu hỏi này nên không thể đưa ra số liệu. "
        "Bạn có thể hỏi lại cụ thể hơn để tôi tra đúng thông tin không?"
    ),
    "stalled_promise": (
        "Xin lỗi, tôi cần tra cứu thêm để trả lời chính xác. Bạn có thể nhắc lại yêu cầu "
        "hoặc cho thêm chi tiết (ví dụ tên/mã sản phẩm cụ thể) để tôi tìm giúp bạn?"
    ),
}


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
    module_expansions: int
    tool_not_found_counts: dict[str, int]
    tools_disabled: bool
    tool_flags_snapshot: dict[str, str]
    model_used: str | None
    known_ids: set[str]
    needs_plan: bool
    plan_id: str | None
    plan_finished: bool


def _latest_human_text(messages) -> str:
    for msg in reversed(messages or []):
        if isinstance(msg, HumanMessage):
            return msg.content
    return ""


def classify_node(state: AgentState) -> dict:
    text = (_latest_human_text(state.get("messages")) or "").lower()
    needs_plan = any(keyword in text for keyword in PLAN_MODE_KEYWORDS)
    return {"needs_plan": needs_plan}


def route_after_classify(state: AgentState) -> str:
    if state.get("plan_id"):
        return "execute_step"
    return "plan" if state.get("needs_plan") else "absorb_steering"


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


_STEP_BLOCK_RE = re.compile(
    r"###\s*BƯỚC\s*\d+\s*:\s*(?P<title>[^\n]+)\n+(?P<detail>.*?)\n+TOOLS:\s*(?P<tools>[^\n]*?)"
    r"(?=\n+###\s*BƯỚC)",
    re.DOTALL,
)
_PLAN_BLOCK_SENTINEL = "\n### BƯỚC 999:"


def _split_plan_blocks(text: str) -> list[dict]:
    blocks = []
    for m in _STEP_BLOCK_RE.finditer(text):
        tools = [t.strip() for t in m.group("tools").split(",") if t.strip()]
        blocks.append({"title": m.group("title").strip(), "detail": m.group("detail").strip(), "tools": tools})
    return blocks


async def _emit_plan_step(client: BackendClient, writer, run_id: str, block: dict) -> None:
    step = await client.add_plan_step(run_id, block["title"], block["detail"], block["tools"])
    writer(("plan_step_added", json.dumps({"step": step}, ensure_ascii=False)))


async def plan_node(state: AgentState) -> dict:
    writer = get_stream_writer()
    client = BackendClient(state["auth_header"])
    run_id = state["run_id"]

    plan_id = state.get("plan_id")
    if not plan_id:
        started = await client.start_plan(run_id, registry_fingerprint())
        plan_id = started["planId"]
        writer(("plan_started", json.dumps({"planId": plan_id}, ensure_ascii=False)))

    current = await client.get_plan(run_id)
    existing_count = len(current["steps"])

    if existing_count >= MAX_PLAN_STEPS:
        await client.mark_plan_ready(run_id)
        writer(("plan_ready", json.dumps({"planId": plan_id}, ensure_ascii=False)))
        return {"plan_id": plan_id}

    request_text = _latest_human_text(state.get("messages"))
    prompt = build_plan_prompt(request_text, current["steps"])

    llm = get_llm(temperature=0.2)
    full_text = ""
    parsed_count = 0
    budget = MAX_PLAN_STEPS - existing_count

    async for chunk in llm.astream(prompt):
        content = chunk if isinstance(chunk, str) else (getattr(chunk, "content", "") or "")
        full_text += content
        blocks = _split_plan_blocks(full_text)
        while parsed_count < len(blocks) and parsed_count < budget:
            await _emit_plan_step(client, writer, run_id, blocks[parsed_count])
            parsed_count += 1

    blocks = _split_plan_blocks(full_text + _PLAN_BLOCK_SENTINEL)
    while parsed_count < len(blocks) and parsed_count < budget:
        await _emit_plan_step(client, writer, run_id, blocks[parsed_count])
        parsed_count += 1

    if parsed_count == 0:
        logger.warning("plan_node không parse được bước nào từ phản hồi LLM cho run %s", run_id)

    await client.mark_plan_ready(run_id)
    writer(("plan_ready", json.dumps({"planId": plan_id}, ensure_ascii=False)))
    return {"plan_id": plan_id}


async def execute_step_node(state: AgentState) -> dict:
    writer = get_stream_writer()
    client = BackendClient(state["auth_header"])
    run_id = state["run_id"]

    current = await client.get_plan(run_id)
    next_step = next((s for s in current["steps"] if s["status"] == "pending"), None)

    if next_step is None:
        return {"current_plan_step": None, "plan_finished": True, "plan_approved": True}

    await client.update_plan_step_status(run_id, next_step["id"], "running")
    writer(("plan_step_started", json.dumps({"stepId": next_step["id"]}, ensure_ascii=False)))

    step_request = HumanMessage(content=f"[BƯỚC KẾ HOẠCH] {next_step['title']}\n{next_step['detail']}")
    return {
        "current_plan_step": next_step,
        "plan_finished": False,
        "messages": [step_request],
        "tool_turns": 0,
        "plan_approved": True,
    }


def route_after_execute_step(state: AgentState) -> str:
    return "summarize" if state.get("plan_finished") else "call_model"


async def step_completed_node(state: AgentState) -> dict:
    writer = get_stream_writer()
    client = BackendClient(state["auth_header"])
    run_id = state["run_id"]
    step = state["current_plan_step"]

    last_message = state["messages"][-1]
    summary = (getattr(last_message, "content", "") or "").strip()

    await client.update_plan_step_status(run_id, step["id"], "done", result=summary[:500])
    writer(("plan_step_completed", json.dumps(
        {"stepId": step["id"], "status": "done", "summary": summary[:200]}, ensure_ascii=False)))

    return {"current_plan_step": None}


async def summarize_node(state: AgentState) -> dict:
    writer = get_stream_writer()
    client = BackendClient(state["auth_header"])
    run_id = state["run_id"]

    current = await client.get_plan(run_id)
    results_text = "\n".join(
        f"- {s['title']}: {s.get('result') or '(không có kết quả)'}"
        for s in current["steps"] if s["status"] == "done"
    ) or "(không có bước nào hoàn tất)"

    llm = get_llm(temperature=0.3)
    prompt = (
        "Tổng hợp kết quả các bước sau thành một câu trả lời hoàn chỉnh, mạch lạc cho người dùng, "
        "bằng tiếng Việt, không liệt kê lại từng bước một cách máy móc:\n" + results_text
    )

    full = ""
    async for chunk in llm.astream(prompt):
        content = chunk if isinstance(chunk, str) else (getattr(chunk, "content", "") or "")
        if content:
            full += content
            writer(("text_delta", content))

    return {"messages": [AIMessage(content=full)]}


async def call_model_node(state: AgentState, config: RunnableConfig) -> dict:
    writer = get_stream_writer()
    client = BackendClient(state["auth_header"])
    llm = get_llm(temperature=0.3)
    updates = {}

    scoping = GUARDRAIL_STATE_KEY in state
    if scoping and state.get("tools_disabled"):
        tools = []
        updates["allowed_tool_names"] = set()
    elif scoping:
        allowed_names = {spec.name for spec in build_tool_scope(state)}
        tools = build_tools(client, allowed_names)
        updates["allowed_tool_names"] = allowed_names
    else:
        tools = build_tools(client)

    if hasattr(llm, "bind_tools"):
        llm = llm.bind_tools(tools)
    cancel_event = config.get("configurable", {}).get("cancel_event")

    full = None
    carried = []
    last_poll = time.monotonic()
    thinking_parser = ThinkingParser()

    async for chunk in llm.astream(state["messages"]):
        if cancel_event is not None and cancel_event.is_set():
            return {"turns": state.get("turns", 0) + 1, "cancelled": True, "carried_steering": []}

        content = chunk if isinstance(chunk, str) else (getattr(chunk, "content", "") or "")
        if content:
            visible = thinking_parser.feed(content)
            if visible:
                writer(("text_delta", visible))

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
        metadata = getattr(full, "response_metadata", None) or {}
        model_used = metadata.get("model_name") or metadata.get("model")
        if model_used:
            updates["model_used"] = model_used

    if thinking_parser.tag_closed:
        if thinking_parser.thinking_text.strip():
            writer(("thinking", json.dumps(
                {"text": _scrub_text(thinking_parser.thinking_text.strip())}, ensure_ascii=False)))
        if isinstance(result_message.content, str):
            tagged = THINKING_OPEN + thinking_parser.thinking_text + THINKING_CLOSE
            result_message.content = result_message.content.replace(tagged, "", 1).lstrip()
    elif isinstance(result_message.content, str) and THINKING_CLOSE in result_message.content:
        leaked_thinking, _, rest = result_message.content.partition(THINKING_CLOSE)
        writer(("message_correction", ""))
        if leaked_thinking.strip():
            writer(("thinking", json.dumps(
                {"text": _scrub_text(leaked_thinking.strip())}, ensure_ascii=False)))
        result_message.content = rest.lstrip()
        if result_message.content:
            writer(("text_delta", result_message.content))

    tool_calls = getattr(result_message, "tool_calls", None)
    if scoping and tool_calls and (result_message.content or "").strip():
        leaked_text = _scrub_text(result_message.content.strip())
        writer(("message_correction", ""))
        writer(("thinking", json.dumps({"text": leaked_text}, ensure_ascii=False)))
        writer(("guardrail_blocked", json.dumps(
            {"tool": "", "reason": "text_kem_tool_call_chuyen_thanh_thinking"}, ensure_ascii=False)))
        result_message = AIMessage(content="", tool_calls=tool_calls)
    elif scoping and not tool_calls:
        guard = check_output(result_message.content or "", {
            "had_forbidden_tool": state.get("had_forbidden_tool", False),
            "tool_call_count": state.get("tool_call_count", 0),
            "has_tools_bound": bool(tools),
        })
        if guard.action == "block":
            result_message = AIMessage(content="Không thể trả lời yêu cầu này.")
            writer(("message_correction", result_message.content))
        elif guard.action == "rewrite":
            result_message = AIMessage(content=REWRITE_MESSAGES.get(guard.kind, REWRITE_MESSAGES["no_permission"]))
            writer(("message_correction", result_message.content))

    return {
        "turns": state.get("turns", 0) + 1,
        "carried_steering": carried,
        "messages": [result_message],
        **updates,
    }


def _envelope_summary(result) -> dict:
    if not isinstance(result, dict):
        return {}
    keys = ("truncated", "totalCount", "asOf", "warnings", "filtersApplied")
    return {k: result[k] for k in keys if k in result}


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
    guarding = "allowed_tool_names" in state
    tools_by_name = {
        tool.name: tool
        for tool in build_tools(client, state["allowed_tool_names"] if guarding else None)
    }
    call_signatures = set(state.get("call_signatures") or set())
    tool_call_count = state.get("tool_call_count", 0)
    had_forbidden_tool = state.get("had_forbidden_tool", False)
    module_expansions = state.get("module_expansions", 0)
    expanded_modules = set(state.get("expanded_modules") or set())
    tool_not_found_counts = dict(state.get("tool_not_found_counts") or {})
    tools_disabled = state.get("tools_disabled", False)
    known_ids = set(state.get("known_ids") or set())
    user_text = _latest_human_text(state.get("messages"))
    specs = load_tool_specs() if guarding else {}

    result_messages = []
    for tool_call in tool_calls:
        name = tool_call["name"]
        args = dict(tool_call["args"])
        summary = describe_args(name, args)
        writer(("tool_start", json.dumps(
            {"name": name, "summary": summary, "argsPreview": make_tool_preview(args)},
            ensure_ascii=False)))

        if guarding:
            lifecycle_error = resolve_tool_call_error(name, {**state, "expanded_modules": expanded_modules}, specs)
            if lifecycle_error is not None:
                kind = lifecycle_error["kind"]
                if kind == "module_expand" and module_expansions < 1:
                    module = lifecycle_error["module"]
                    expanded_modules.add(module)
                    module_expansions += 1
                    reason = "module_loaded"
                    content = json.dumps({"info": reason,
                                           "message": f"Đã nạp thêm nhóm tool '{module}'. Hãy gọi lại."},
                                          ensure_ascii=False)
                else:
                    if kind == "module_expand":
                        message = (f"Bạn không có quyền dùng '{name}'. Hãy nói với người dùng rằng họ "
                                   "không có quyền truy cập thông tin này. KHÔNG đoán dữ liệu.")
                    else:
                        message = lifecycle_error["message"]
                    if kind == "tool_not_found":
                        tool_not_found_counts[name] = tool_not_found_counts.get(name, 0) + 1
                        if tool_not_found_counts[name] >= 2:
                            message += " Dừng thử tool này, hãy trả lời bằng dữ liệu đã có."
                            tools_disabled = True
                    reason = message
                    content = json.dumps({"error": message}, ensure_ascii=False)
                result_messages.append(ToolMessage(content=content, tool_call_id=tool_call["id"]))
                writer(("tool_end", json.dumps({"name": name}, ensure_ascii=False)))
                writer(("guardrail_blocked", json.dumps({"tool": name, "reason": reason}, ensure_ascii=False)))
                continue

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

            id_error = check_known_id(name, args, {"known_ids": known_ids, "user_text": user_text})
            if id_error is not None:
                result_messages.append(ToolMessage(
                    content=json.dumps({"error": id_error}, ensure_ascii=False),
                    tool_call_id=tool_call["id"],
                ))
                writer(("tool_end", json.dumps({"name": name}, ensure_ascii=False)))
                writer(("guardrail_blocked", json.dumps(
                    {"tool": name, "reason": id_error}, ensure_ascii=False)))
                continue

            args = guard.args
            call_signatures.add(call_signature(name, args))
            tool_call_count += 1

        tool = tools_by_name.get(name)
        result = None
        started = time.perf_counter()
        try:
            if tool is None:
                raise ValueError(f"Unknown tool: {name}")
            result = await tool.ainvoke(args)
            if guarding:
                known_ids.update(extract_produced_ids(name, result))
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
        result_for_ui = result if isinstance(result, dict) else {"error": content}
        writer(("tool_end", json.dumps({
            "name": name,
            "durationMs": int((time.perf_counter() - started) * 1000),
            "summary": _scrub_text(summarize_result(name, result_for_ui)),
            "resultPreview": make_tool_preview(result_for_ui),
            **_envelope_summary(result),
        }, ensure_ascii=False)))

    updates = {"tool_turns": tool_turns, "tool_limit_reached": False, "messages": result_messages}
    if guarding:
        updates["call_signatures"] = call_signatures
        updates["tool_call_count"] = tool_call_count
        updates["had_forbidden_tool"] = had_forbidden_tool
        updates["module_expansions"] = module_expansions
        updates["expanded_modules"] = expanded_modules
        updates["tool_not_found_counts"] = tool_not_found_counts
        updates["tools_disabled"] = tools_disabled
        updates["known_ids"] = known_ids
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
    if state.get("carried_steering"):
        return "absorb_steering"
    if state.get("current_plan_step") is not None:
        return "step_completed"
    return "absorb_steering"


def route_after_tools(state: AgentState) -> str:
    if state.get("tool_limit_reached"):
        if state.get("current_plan_step") is not None:
            return "step_completed"
        return "absorb_steering"
    return "call_model"


def build_graph():
    graph = StateGraph(AgentState)
    graph.add_node("classify", classify_node)
    graph.add_node("plan", plan_node)
    graph.add_node("execute_step", execute_step_node)
    graph.add_node("step_completed", step_completed_node)
    graph.add_node("summarize", summarize_node)
    graph.add_node("absorb_steering", absorb_steering_node)
    graph.add_node("call_model", call_model_node)
    graph.add_node("call_tools", call_tools_node)

    graph.set_entry_point("classify")
    graph.add_conditional_edges("classify", route_after_classify, {
        "plan": "plan",
        "execute_step": "execute_step",
        "absorb_steering": "absorb_steering",
    })

    graph.add_edge("plan", END)
    graph.add_conditional_edges("execute_step", route_after_execute_step, {
        "call_model": "call_model",
        "summarize": "summarize",
    })
    graph.add_edge("step_completed", "execute_step")
    graph.add_edge("summarize", END)

    graph.add_conditional_edges("absorb_steering", route_after_absorb, {
        "continue": "call_model",
        "end": END,
    })
    graph.add_conditional_edges("call_model", route_after_model, {
        "call_tools": "call_tools",
        "step_completed": "step_completed",
        "absorb_steering": "absorb_steering",
    })
    graph.add_conditional_edges("call_tools", route_after_tools, {
        "call_model": "call_model",
        "step_completed": "step_completed",
        "absorb_steering": "absorb_steering",
    })
    return graph.compile(checkpointer=MemorySaver())


@lru_cache
def get_graph():
    return build_graph()
