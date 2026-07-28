import asyncio
import json
import logging

from fastapi import APIRouter, Depends, Request, HTTPException
from fastapi.responses import StreamingResponse
from langchain_core.messages import HumanMessage

from app.agents.manager_agent import get_graph
from app.api.deps import verify_internal_secret
from app.schemas.chat import ChatRequest, GenerateTitleRequest
from app.services.backend_client import BackendClient
from app.services.prompt_builder import build_system_message, build_history_messages

logger = logging.getLogger(__name__)

router = APIRouter()

_cancel_events: dict[str, asyncio.Event] = {}


@router.post("/manager-chat/generate-title")
async def generate_title(req: GenerateTitleRequest, _: str = Depends(verify_internal_secret)):
    title = req.message[:30].strip() + ("..." if len(req.message) > 30 else "")
    return {"title": title}


@router.post("/manager-chat/{run_id}/cancel")
async def cancel_chat(run_id: str, _: str = Depends(verify_internal_secret)):
    event = _cancel_events.get(run_id)
    if event:
        event.set()
    return {"cancelled": True}


def _event(type_: str, payload: str = "") -> str:
    return json.dumps({"type": type_, "payload": payload}) + "\n"


@router.post("/manager-chat")
async def handle_chat(request: Request, chat_req: ChatRequest, _: str = Depends(verify_internal_secret)):
    auth_header = request.headers.get("Authorization")
    if not auth_header:
        raise HTTPException(status_code=401, detail="Missing Authorization header")

    context = None
    try:
        client = BackendClient(auth_header)
        context = await client.get_context(chat_req.session_id, chat_req.message)
    except Exception:
        logger.warning("Failed to fetch context for session %s", chat_req.session_id, exc_info=True)

    initial_state = {
        "messages": [
            build_system_message(context),
            *build_history_messages(context, chat_req.message),
            HumanMessage(content=chat_req.message),
        ],
        "run_id": chat_req.run_id,
        "auth_header": auth_header,
        "turns": 0,
        "absorbed_count": 0,
        "carried_steering": [],
        "cancelled": False,
    }

    cancel_event = asyncio.Event()
    _cancel_events[chat_req.run_id] = cancel_event

    graph = get_graph()
    config = {"configurable": {"thread_id": chat_req.run_id, "cancel_event": cancel_event}}

    async def stream_generator():
        try:
            async for type_, payload in graph.astream(initial_state, config=config, stream_mode="custom"):
                yield _event(type_, payload)
            yield _event("done")
        except Exception as e:
            logger.error("LLM streaming error for run %s: %s", chat_req.run_id, str(e))
            yield _event("error", "Đã có lỗi xảy ra khi kết nối tới AI. Vui lòng thử lại.")
        finally:
            _cancel_events.pop(chat_req.run_id, None)

    return StreamingResponse(stream_generator(), media_type="application/x-ndjson")
