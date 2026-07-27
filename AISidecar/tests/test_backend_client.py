import pytest
import httpx
import respx

from app.core.errors import BackendError, ForbiddenError
from app.services.backend_client import BackendClient


@pytest.fixture
def backend_client(monkeypatch):
    monkeypatch.setenv("BACKEND_URL", "http://testhost:5000/api")
    monkeypatch.setenv("BACKEND_INTERNAL_SECRET", "test-secret")
    return BackendClient("Bearer fake-token")


@pytest.mark.asyncio
@respx.mock
async def test_get_context_thanh_cong(backend_client):
    respx.post("http://testhost:5000/internal/chat/context").mock(
        return_value=httpx.Response(200, json={"user": "test"})
    )
    result = await backend_client.get_context("s1", "hello")
    assert result == {"user": "test"}


@pytest.mark.asyncio
@respx.mock
async def test_403_ném_forbidden_error(backend_client):
    respx.post("http://testhost:5000/internal/chat/context").mock(
        return_value=httpx.Response(403)
    )
    with pytest.raises(ForbiddenError):
        await backend_client.get_context("s1", "hello")


@pytest.mark.asyncio
@respx.mock
async def test_500_ném_backend_error(backend_client):
    respx.post("http://testhost:5000/internal/chat/context").mock(
        return_value=httpx.Response(500)
    )
    with pytest.raises(BackendError) as exc_info:
        await backend_client.get_context("s1", "hello")
    assert exc_info.value.status == 500


@pytest.mark.asyncio
@respx.mock
async def test_header_internal_secret_duoc_gui(backend_client):
    route = respx.post("http://testhost:5000/internal/chat/context").mock(
        return_value=httpx.Response(200, json={})
    )
    await backend_client.get_context("s1", "hello")
    assert route.calls[0].request.headers["x-internal-secret"] == "test-secret"
    assert route.calls[0].request.headers["authorization"] == "Bearer fake-token"


@pytest.mark.asyncio
@respx.mock
async def test_call_tool(backend_client):
    respx.post("http://testhost:5000/internal/chat/tools/products/search").mock(
        return_value=httpx.Response(200, json={"items": []})
    )
    result = await backend_client.call_tool("products/search", {"q": "honda"})
    assert result == {"items": []}
