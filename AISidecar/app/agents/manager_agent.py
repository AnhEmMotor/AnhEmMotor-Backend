import time
from functools import lru_cache
from typing import Annotated, TypedDict

from langchain_core.messages import AIMessage, HumanMessage
from langchain_core.runnables import RunnableConfig
from langgraph.checkpoint.memory import MemorySaver
from langgraph.config import get_stream_writer
from langgraph.graph import END, StateGraph
from langgraph.graph.message import add_messages

from app.core.llm import get_llm
from app.services.backend_client import BackendClient

STEERING_POLL_INTERVAL_SECONDS = 0.7


class AgentState(TypedDict):
    messages: Annotated[list, add_messages]
    run_id: str
    auth_header: str
    turns: int
    absorbed_count: int
    carried_steering: list
    cancelled: bool


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

    if not pending:
        return {"absorbed_count": 0, "carried_steering": []}

    writer = get_stream_writer()
    writer(("turn_boundary", ""))
    new_messages = [build_steering_message(item) for item in pending]
    for item in pending:
        if item["mode"] == "interrupt":
            writer(("run_redirected", "user_correction"))

    return {"messages": new_messages, "absorbed_count": len(pending), "carried_steering": []}


async def call_model_node(state: AgentState, config: RunnableConfig) -> dict:
    writer = get_stream_writer()
    llm = get_llm(temperature=0.7)
    client = BackendClient(state["auth_header"])
    cancel_event = config.get("configurable", {}).get("cancel_event")

    chunks = []
    carried = []
    last_poll = time.monotonic()

    async for chunk in llm.astream(state["messages"]):
        if cancel_event is not None and cancel_event.is_set():
            return {"turns": state.get("turns", 0) + 1, "cancelled": True, "carried_steering": []}

        content = chunk if isinstance(chunk, str) else (getattr(chunk, "content", "") or "")
        if content:
            chunks.append(content)
            writer(("text_delta", content))

        now = time.monotonic()
        if now - last_poll >= STEERING_POLL_INTERVAL_SECONDS:
            last_poll = now
            pulled = await client.pull_pending_steering(state["run_id"])
            if pulled:
                carried.extend(pulled)
                if any(item["mode"] == "interrupt" for item in pulled):
                    return {"turns": state.get("turns", 0) + 1, "carried_steering": carried}

    return {
        "turns": state.get("turns", 0) + 1,
        "carried_steering": carried,
        "messages": [AIMessage(content="".join(chunks))],
    }


def route_after_absorb(state: AgentState) -> str:
    if state.get("cancelled"):
        return "end"
    if state.get("turns", 0) == 0:
        return "continue"
    if state.get("absorbed_count", 0) > 0:
        return "continue"
    return "end"


def build_graph():
    graph = StateGraph(AgentState)
    graph.add_node("absorb_steering", absorb_steering_node)
    graph.add_node("call_model", call_model_node)
    graph.set_entry_point("absorb_steering")
    graph.add_conditional_edges("absorb_steering", route_after_absorb, {
        "continue": "call_model",
        "end": END,
    })
    graph.add_edge("call_model", "absorb_steering")
    return graph.compile(checkpointer=MemorySaver())


@lru_cache
def get_graph():
    return build_graph()
