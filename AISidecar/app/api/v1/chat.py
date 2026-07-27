import logging

from fastapi import APIRouter, Depends, Request, HTTPException
from fastapi.responses import StreamingResponse
from langchain_core.messages import HumanMessage

from app.api.deps import verify_internal_secret
from app.core.llm import get_llm
from app.schemas.chat import ChatRequest, GenerateTitleRequest
from app.services.backend_client import BackendClient
from app.services.prompt_builder import build_system_message, build_history_messages

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

    context = None
    try:
        client = BackendClient(auth_header)
        context = await client.get_context(chat_req.session_id, chat_req.message)
    except Exception:
        logger.warning("Failed to fetch context for session %s", chat_req.session_id, exc_info=True)

    messages = [
        build_system_message(context),
        *build_history_messages(context, chat_req.message),
        HumanMessage(content=chat_req.message),
    ]

    llm = get_llm(temperature=0.7)

    async def stream_generator():
        try:
            async for chunk in llm.astream(messages):
                if isinstance(chunk, str):
                    yield chunk
                elif hasattr(chunk, "content"):
                    yield chunk.content
                else:
                    yield str(chunk)
        except Exception as e:
            logger.error("LLM streaming error for session %s: %s", chat_req.session_id, str(e))
            yield "\n[Đã có lỗi xảy ra khi kết nối tới AI. Vui lòng thử lại.]"

    return StreamingResponse(stream_generator(), media_type="text/plain")
