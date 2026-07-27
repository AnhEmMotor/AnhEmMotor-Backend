import logging

from fastapi import APIRouter, Depends, Request, HTTPException
from fastapi.responses import StreamingResponse
from langchain_core.messages import SystemMessage, HumanMessage

from app.api.deps import verify_internal_secret
from app.core.llm import get_llm
from app.prompts.loader import render
from app.schemas.chat import ChatRequest, GenerateTitleRequest
from app.services.backend_client import BackendClient

logger = logging.getLogger(__name__)

router = APIRouter()


@router.post("/manager-chat/generate-title")
async def generate_title(req: GenerateTitleRequest, _: str = Depends(verify_internal_secret)):
    title = req.message[:30].strip() + ("..." if len(req.message) > 30 else "")
    return {"title": title}


@router.post("/manager-chat")
async def handle_chat(request: Request, chat_req: ChatRequest, _: str = Depends(verify_internal_secret)):
    auth_header = request.headers.get("Authorization")
    if not auth_header:
        raise HTTPException(status_code=401, detail="Missing Authorization header")

    context = {}
    try:
        client = BackendClient(auth_header)
        context = await client.get_context(chat_req.session_id, chat_req.message)
    except Exception:
        logger.exception("Failed to fetch context for session %s", chat_req.session_id)

    llm = get_llm(temperature=0.7)
    system_prompt = render("system_manager_chat")
    messages = [
        SystemMessage(content=system_prompt),
        HumanMessage(content=chat_req.message)
    ]

    async def stream_generator():
        try:
            async for chunk in llm.astream(messages):
                if isinstance(chunk, str):
                    yield chunk
                elif hasattr(chunk, "content"):
                    yield chunk.content
                else:
                    yield str(chunk)
        except Exception:
            logger.exception("LLM streaming error for session %s", chat_req.session_id)
            yield "\n[Đã có lỗi xảy ra khi kết nối tới AI. Vui lòng thử lại.]"

    return StreamingResponse(stream_generator(), media_type="text/plain")
