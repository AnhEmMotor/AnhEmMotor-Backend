import httpx
import pytest
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


@pytest.mark.asyncio
@respx.mock
async def test_call_tool_doi_key_snake_case_sang_camel_case(backend_client):
    route = respx.post("http://testhost:5000/internal/chat/tools/orders/status").mock(
        return_value=httpx.Response(200, json={})
    )
    await backend_client.call_tool("orders/status", {"order_id": 1024, "from_date": "2026-07-01"})
    sent_body = route.calls[0].request.content
    import json
    assert json.loads(sent_body) == {"orderId": 1024, "fromDate": "2026-07-01"}


@pytest.mark.asyncio
@respx.mock
async def test_update_routing_context_khong_loi_khi_response_rong(backend_client):
    respx.post("http://testhost:5000/internal/chat/sessions/s1/routing-context").mock(
        return_value=httpx.Response(200)
    )
    await backend_client.update_routing_context("s1", {"lastModules": ["sales"]})


@pytest.mark.asyncio
@respx.mock
async def test_find_plan_template_tra_none_khi_404(backend_client):
    respx.get("http://testhost:5000/internal/chat/plan-templates/find").mock(
        return_value=httpx.Response(404)
    )
    result = await backend_client.find_plan_template("hash1", "sales")
    assert result is None


@pytest.mark.asyncio
@respx.mock
async def test_find_plan_template_tra_du_lieu_khi_co(backend_client):
    respx.get("http://testhost:5000/internal/chat/plan-templates/find").mock(
        return_value=httpx.Response(200, json={"templateId": "t1", "module": "sales"})
    )
    result = await backend_client.find_plan_template("hash1", "sales")
    assert result == {"templateId": "t1", "module": "sales"}


@pytest.mark.asyncio
@respx.mock
async def test_create_plan_template_goi_dung_endpoint(backend_client):
    route = respx.post("http://testhost:5000/internal/chat/plan-templates").mock(
        return_value=httpx.Response(200, json={"templateId": "t1"})
    )
    result = await backend_client.create_plan_template({"canonicalQuestion": "doanh thu tháng này"})
    assert result == {"templateId": "t1"}
    assert route.called


@pytest.mark.asyncio
@respx.mock
async def test_record_plan_template_use_goi_dung_payload(backend_client):
    route = respx.post("http://testhost:5000/internal/chat/plan-templates/t1/record-use").mock(
        return_value=httpx.Response(200)
    )
    await backend_client.record_plan_template_use("t1", success=True, user_edited=False, rejected=False)
    import json
    assert json.loads(route.calls[0].request.content) == {
        "success": True, "userEdited": False, "rejected": False,
    }
