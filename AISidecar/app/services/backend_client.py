import json
import logging

import httpx

from app.config import get_settings
from app.core.errors import BackendError, ForbiddenError

logger = logging.getLogger(__name__)


def _to_camel_case(snake_str: str) -> str:
    parts = snake_str.split("_")
    return parts[0] + "".join(p.title() for p in parts[1:])


class BackendClient:
    def __init__(self, auth_header: str):
        self._settings = get_settings()
        self._auth_header = auth_header

    def _headers(self) -> dict:
        return {
            "Authorization": self._auth_header,
            "X-Internal-Secret": self._settings.backend_internal_secret,
        }

    async def _post(self, path: str, payload: dict) -> dict | list:
        url = f"{self._settings.backend_base}{path}"
        timeout = self._settings.request_timeout_seconds
        async with httpx.AsyncClient(timeout=timeout) as client:
            resp = await client.post(url, json=payload, headers=self._headers())
        if resp.status_code == 403:
            raise ForbiddenError(path)
        if resp.status_code >= 400:
            raise BackendError(path, resp.status_code)
        if not resp.content:
            return {}
        return resp.json()

    async def get_tool_manifest(self) -> dict:
        url = f"{self._settings.backend_base}/internal/chat/tools/manifest"
        timeout = self._settings.request_timeout_seconds
        async with httpx.AsyncClient(timeout=timeout) as client:
            resp = await client.get(url, headers=self._headers())
        if resp.status_code >= 400:
            raise BackendError("/internal/chat/tools/manifest", resp.status_code)
        return resp.json()

    async def get_context(self, session_id: str, message: str,
                          history_limit: int = 20) -> dict:
        return await self._post("/internal/chat/context", {
            "sessionId": session_id,
            "message": message,
            "historyLimit": history_limit,
        })

    async def call_tool(self, tool_path: str, payload: dict) -> dict:
        camel_payload = {_to_camel_case(k): v for k, v in payload.items()}
        return await self._post(
            f"/internal/chat/tools/{tool_path.lstrip('/')}", camel_payload
        )

    async def call_public_tool(self, tool_path: str, payload: dict) -> dict:
        camel_payload = {_to_camel_case(k): v for k, v in payload.items()}
        return await self._post(
            f"/internal/chat/tools/store/{tool_path.lstrip('/')}", camel_payload
        )

    async def pull_pending_steering(self, run_id: str) -> list[dict]:
        result = await self._post(f"/internal/chat/runs/{run_id}/pull-steering", {})
        return result if isinstance(result, list) else []

    async def update_routing_context(self, session_id: str, routing_context: dict) -> None:
        await self._post(f"/internal/chat/sessions/{session_id}/routing-context", {
            "routingContext": json.dumps(routing_context, ensure_ascii=False, default=str),
        })


