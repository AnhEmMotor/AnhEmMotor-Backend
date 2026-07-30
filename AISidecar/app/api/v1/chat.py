import asyncio
import json
import logging
from datetime import datetime

from fastapi import APIRouter, Depends, Request, HTTPException
from fastapi.responses import StreamingResponse
from langchain_core.messages import HumanMessage

from app.agents.manager_agent import get_graph
from app.api.deps import verify_internal_secret
from app.config import get_settings
from app.schemas.chat import ChatRequest, GenerateTitleRequest
from app.services.backend_client import BackendClient
from app.services.prompt_builder import build_system_message, build_history_messages
from app.services.routing import expire_if_stale, extract_entities
from app.tools.registry import registry_fingerprint

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
    client = BackendClient(auth_header)
    try:
        context = await client.get_context(chat_req.session_id, chat_req.message)
    except Exception:
        logger.warning("Failed to fetch context for session %s", chat_req.session_id, exc_info=True)

    routing_context = {}
    if context:
        try:
            routing_context = json.loads(context.get("routingContext") or "{}")
        except ValueError:
            routing_context = {}
        routing_context = expire_if_stale(routing_context, now=datetime.now().isoformat())

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
        "tool_turns": 0,
        "permissions": (context or {}).get("permissions") or [],
        "history": (context or {}).get("history") or [],
        "routing_context": routing_context,
        "tool_flags_snapshot": dict(get_settings().tool_flags),
    }

    cancel_event = asyncio.Event()
    _cancel_events[chat_req.run_id] = cancel_event

    graph = get_graph()
    config = {"configurable": {"thread_id": chat_req.run_id, "cancel_event": cancel_event}}

    async def stream_generator():
        try:
            async for type_, payload in graph.astream(initial_state, config=config, stream_mode="custom"):
                yield _event(type_, payload)
            final_state = graph.get_state(config).values
            run_meta = {
                "toolRegistryFingerprint": registry_fingerprint(),
                "modelUsed": final_state.get("model_used"),
            }
            yield _event("run_meta", json.dumps(run_meta, ensure_ascii=False))
            yield _event("done")
        except Exception as e:
            logger.error("LLM streaming error for run %s: %s", chat_req.run_id, str(e))
            yield _event("error", "Đã có lỗi xảy ra khi kết nối tới AI. Vui lòng thử lại.")
        finally:
            _cancel_events.pop(chat_req.run_id, None)
            await _persist_routing_context(client, graph, config, chat_req.session_id, routing_context)

    return StreamingResponse(stream_generator(), media_type="application/x-ndjson")


async def _persist_routing_context(client: BackendClient, graph, config, session_id: str, previous: dict) -> None:
    try:
        final = graph.get_state(config).values
    except Exception:
        return
    tool_calls_made = [
        tc for m in (final.get("messages") or [])
        for tc in (getattr(m, "tool_calls", None) or [])
    ]
    if not tool_calls_made and not final.get("scoped_modules"):
        return
    entities = {**(previous.get("entities") or {}), **extract_entities(tool_calls_made)}
    updated = {
        "entities": entities,
        "lastModules": final.get("scoped_modules") or previous.get("lastModules") or [],
        "updatedAt": datetime.now().isoformat(),
        "turnCount": (previous.get("turnCount") or 0) + 1,
    }
    try:
        await client.update_routing_context(session_id, updated)
    except Exception:
        logger.warning("Failed to persist routing context for session %s", session_id, exc_info=True)
