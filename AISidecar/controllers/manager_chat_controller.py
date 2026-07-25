from fastapi import APIRouter, Request, HTTPException
from fastapi.responses import StreamingResponse
import asyncio
import httpx
import json
from schemas.chat_schemas import ChatRequest, GenerateTitleRequest

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
    reply = json.dumps(context, ensure_ascii=False, indent=2)
    async def stream_generator():
        words = reply.split(" ")
        for i, word in enumerate(words):
            yield word + (" " if i < len(words) - 1 else "")
            await asyncio.sleep(0.01)
    return StreamingResponse(stream_generator(), media_type="text/plain")
