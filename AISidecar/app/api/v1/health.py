import time

from fastapi import APIRouter

from app.services.backend_client import BackendClient
from app.tools.registry import verify_tool_contract

router = APIRouter()

_STALE_CACHE_TTL_SECONDS = 60
_stale_cache = {"checked_at": 0.0, "stale": False}


@router.get("/")
def read_root():
    return {"status": "ok", "message": "AI Sidecar is running"}


@router.get("/health")
async def health():
    now = time.monotonic()
    if now - _stale_cache["checked_at"] >= _STALE_CACHE_TTL_SECONDS:
        result = await verify_tool_contract(BackendClient(""))
        _stale_cache["stale"] = result["stale_build"]
        _stale_cache["checked_at"] = now
    return {"status": "ok", "stale": _stale_cache["stale"]}
