import json

from langchain_core.messages import AIMessageChunk

from app.agents import store_agent
from app.services.store_tools import STORE_TOOL_NAMES


def _fake_envelope(items):
    return {
        "items": items,
        "totalCount": len(items),
        "truncated": False,
        "asOf": "2026-07-26T09:15:00+07:00",
        "timezone": "Asia/Ho_Chi_Minh",
        "source": "fake",
        "filtersApplied": {},
        "definition": None,
        "currency": None,
        "warnings": [],
    }


class FakeStoreBackendClient:
    def __init__(self, auth_header=""):
        self.calls = []

    async def call_public_tool(self, tool_path, payload):
        self.calls.append((tool_path, payload))
        if tool_path == "products/search":
            return _fake_envelope([{
                "productId": 123, "productName": "Honda SH 2024", "brandName": "Honda",
                "categoryName": "Xe tay ga", "priceFrom": 89000000, "priceTo": 95000000,
                "currency": "VND", "variantCount": 2,
            }])
        if tool_path == "products/detail":
            return _fake_envelope([{
                "productId": 123, "productName": "Honda SH 2024", "brandName": "Honda",
                "categoryName": "Xe tay ga", "priceFrom": 89000000, "priceTo": 95000000, "currency": "VND",
                "variants": [
                    {"variantId": 456, "variantName": "Đỏ đen", "sku": "SH24-RB", "price": 91000000, "slug": "sh-2024-do-den"},
                ],
            }])
        return _fake_envelope([])


def _tool_call(name, args, call_id):
    return {"name": name, "args": args, "id": call_id, "type": "tool_call"}


def _base_state(extra=None):
    state = {
        "messages": [],
        "tool_turns": 0,
        "tool_limit_reached": False,
        "tool_call_count": 0,
        "call_signatures": set(),
    }
    state.update(extra or {})
    return state


def test_store_tool_names_khop_dung_5_tool():
    assert STORE_TOOL_NAMES == {
        "search_products", "get_product_detail", "get_product_stock",
        "get_product_price_list", "list_brands",
    }


async def test_call_tools_node_tu_choi_tool_noi_bo_khong_goi_backend(monkeypatch):
    fake_client = FakeStoreBackendClient()
    monkeypatch.setattr(store_agent, "BackendClient", lambda auth_header="": fake_client)
    events = []
    monkeypatch.setattr(store_agent, "get_stream_writer", lambda: events.append)

    state = _base_state({
        "messages": [AIMessageChunk(
            content="",
            tool_calls=[_tool_call("get_staff_performance", {}, "call-1")],
        )],
    })
    result = await store_agent.call_tools_node(state)

    assert fake_client.calls == []
    tool_message = result["messages"][0]
    payload = json.loads(tool_message.content)
    assert "error" in payload
    assert "không nằm trong phạm vi" in payload["error"]
    blocked_events = [e for e in events if e[0] == "guardrail_blocked"]
    assert json.loads(blocked_events[0][1])["reason"] == "tool_not_in_store_scope"


async def test_call_tools_node_search_products_emit_product_cards(monkeypatch):
    fake_client = FakeStoreBackendClient()
    monkeypatch.setattr(store_agent, "BackendClient", lambda auth_header="": fake_client)
    events = []
    monkeypatch.setattr(store_agent, "get_stream_writer", lambda: events.append)

    state = _base_state({
        "messages": [AIMessageChunk(
            content="",
            tool_calls=[_tool_call("search_products", {"keyword": "SH"}, "call-1")],
        )],
    })
    await store_agent.call_tools_node(state)

    assert fake_client.calls == [("products/search", {"keyword": "SH", "limit": 10})]
    card_events = [e for e in events if e[0] == "product-cards"]
    assert len(card_events) == 1
    cards = json.loads(card_events[0][1])
    assert cards["items"][0]["productId"] == 123
    assert cards["items"][0]["name"] == "Honda SH 2024"
    assert cards["items"][0]["priceFrom"] == 89000000


async def test_call_tools_node_get_product_detail_emit_variant_cards(monkeypatch):
    fake_client = FakeStoreBackendClient()
    monkeypatch.setattr(store_agent, "BackendClient", lambda auth_header="": fake_client)
    events = []
    monkeypatch.setattr(store_agent, "get_stream_writer", lambda: events.append)

    state = _base_state({
        "messages": [AIMessageChunk(
            content="",
            tool_calls=[_tool_call("get_product_detail", {"product_id": 123}, "call-1")],
        )],
    })
    await store_agent.call_tools_node(state)

    variant_events = [e for e in events if e[0] == "variant-cards"]
    assert len(variant_events) == 1
    payload = json.loads(variant_events[0][1])
    assert payload["productId"] == 123
    assert payload["items"][0] == {
        "variantId": 456, "colorName": "Đỏ đen", "sku": "SH24-RB", "price": 91000000, "slug": "sh-2024-do-den",
    }


async def test_call_tools_node_vuot_nguong_tool_turns_dung_lai(monkeypatch):
    monkeypatch.setattr(store_agent, "get_stream_writer", lambda: (lambda *_: None))
    state = _base_state({
        "tool_turns": store_agent.MAX_TOOL_TURNS,
        "messages": [AIMessageChunk(
            content="",
            tool_calls=[_tool_call("search_products", {"keyword": "SH"}, "call-1")],
        )],
    })
    result = await store_agent.call_tools_node(state)
    assert result["tool_limit_reached"] is True
    assert store_agent.route_after_tools(result) == "end"
