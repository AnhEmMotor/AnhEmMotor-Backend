from fastapi import APIRouter, Request, HTTPException
from fastapi.responses import StreamingResponse
import asyncio
import httpx
import json
from schemas.chat_schemas import ChatRequest, GenerateTitleRequest
from services.llm_factory import get_llm
from langchain_core.messages import SystemMessage, HumanMessage

router = APIRouter()

@router.post("/manager-chat/generate-title")
async def generate_title(req: GenerateTitleRequest):
    title = req.message[:30].strip() + ("..." if len(req.message) > 30 else "")
    return {"title": title}

@router.post("/manager-chat")
async def handle_chat(request: Request, chat_req: ChatRequest):
    auth_header = request.headers.get("Authorization")
    if not auth_header:
        raise HTTPException(status_code=401, detail="Missing Authorization header")
    context = {}
    try:
        async with httpx.AsyncClient() as client:
            headers = {"Authorization": auth_header}
            payload = {
                "sessionId": chat_req.session_id,
                "message": chat_req.message
            }
            import os
            backend_base_url = os.environ.get("BACKEND_URL", "http://localhost:5000/api")
            base_url = backend_base_url.rstrip('/').replace('/api', '')
            final_url = f"{base_url}/internal/chat/context"
            response = await client.post(final_url, json=payload, headers=headers)
            if response.status_code == 200:
                context = response.json()
    except Exception:
        pass
        
    llm = get_llm(temperature=0.7)
    
    system_prompt = f"Bạn là trợ lý AI cho ứng dụng AnhEmMotor. Hãy trả lời câu hỏi của người dùng một cách thân thiện và chính xác dựa trên ngữ cảnh được cung cấp."
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
        except Exception as e:
            yield f"\n[Lỗi kết nối tới AI Provider: {str(e)}]"
            
    return StreamingResponse(stream_generator(), media_type="text/plain")
