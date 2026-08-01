import json
import logging
import uuid

from fastapi import APIRouter, Depends
from fastapi.responses import StreamingResponse
from langchain_core.messages import HumanMessage

from app.agents.store_agent import get_store_graph
from app.api.deps import verify_internal_secret
from app.schemas.chat import StoreChatRequest
from app.services.prompt_builder import build_history_messages, build_store_system_message

logger = logging.getLogger(__name__)

router = APIRouter()


def _event(type_: str, payload: str = "") -> str:
    return json.dumps({"type": type_, "payload": payload}) + "\n"


@router.post("/store-chat")
async def handle_store_chat(chat_req: StoreChatRequest, _: str = Depends(verify_internal_secret)):
    initial_state = {
        "messages": [
            build_store_system_message(chat_req.server_date),
            *build_history_messages({"history": chat_req.history}, chat_req.message),
            HumanMessage(content=chat_req.message),
        ],
        "tool_turns": 0,
        "tool_limit_reached": False,
        "tool_call_count": 0,
        "call_signatures": set(),
    }

    graph = get_store_graph()
    config = {"configurable": {"thread_id": str(uuid.uuid4())}}

    async def stream_generator():
        try:
            async for type_, payload in graph.astream(initial_state, config=config, stream_mode="custom"):
                yield _event(type_, payload)
            yield _event("done")
        except Exception as e:
            logger.error("Store chat streaming error: %s", str(e))
            yield _event("error", "Đã có lỗi xảy ra khi kết nối tới AI. Vui lòng thử lại.")

    return StreamingResponse(stream_generator(), media_type="application/x-ndjson")
