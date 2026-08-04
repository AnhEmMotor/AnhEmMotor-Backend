from app.agents import manager_agent
from app.services import qdrant_client as qc
from app.services.backend_client import BackendClient
from app.services.chat_tools import build_all_tools
from app.tools import knowledge


class FakeBackendClient:
    def __init__(self):
        self.calls = []

    async def call_tool(self, tool_path, payload):
        self.calls.append((tool_path, payload))
        product_id = payload["product_id"]
        if tool_path == "products/detail":
            return {"items": [{
                "productId": product_id, "productName": f"SP {product_id}",
                "priceFrom": 90_000_000, "priceTo": 90_000_000,
            }], "totalCount": 1, "truncated": False}
        return {"items": [{"productId": product_id, "stockQuantity": 12}], "totalCount": 1, "truncated": False}


async def test_gia_va_ton_kho_lay_lai_tu_sql(monkeypatch):
    monkeypatch.setattr(qc, "search_products", _fake_search_products)
    backend = FakeBackendClient()
    tool = knowledge._make_semantic_product_search(backend)

    result = await tool(query="xe ga")

    assert result["items"][0]["priceFrom"] == 90_000_000
    assert result["items"][0]["stockItems"][0]["stockQuantity"] == 12


async def _fake_search_products(query, max_price=None, in_stock_only=True, limit=8):
    return [{"productId": "1", "score": 0.7}]


def test_rag_tat_thi_khong_co_tool_knowledge(monkeypatch):
    monkeypatch.delenv("QDRANT_URL", raising=False)
    client = BackendClient("Bearer x")
    names = {t.name for t in build_all_tools(client)}
    assert "search_knowledge" not in names
    assert "semantic_product_search" not in names
    assert "search_products" in names


def test_rag_bat_thi_co_tool_knowledge(monkeypatch):
    monkeypatch.setenv("QDRANT_URL", "http://localhost:6333")
    client = BackendClient("Bearer x")
    names = {t.name for t in build_all_tools(client)}
    assert "search_knowledge" in names
    assert "semantic_product_search" in names


async def test_goi_search_knowledge_gom_citation_vao_state(monkeypatch):
    from langchain_core.messages import AIMessageChunk

    monkeypatch.setenv("QDRANT_URL", "http://localhost:6333")

    class FakeBackendClientNoOp:
        def __init__(self, auth_header):
            pass

        async def pull_pending_steering(self, run_id):
            return []

    async def fake_search_knowledge(query, limit=5):
        return [{"citationId": "c1", "sourceFile": "warranty.md", "heading": "Bảo hành", "content": "..."}]

    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClientNoOp)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    monkeypatch.setattr(qc, "search_knowledge", fake_search_knowledge)

    state = {
        "messages": [AIMessageChunk(content="", tool_calls=[
            {"name": "search_knowledge", "args": {"query": "bảo hành"}, "id": "c1", "type": "tool_call"},
        ])],
        "run_id": "r1",
        "auth_header": "Bearer x",
        "tool_turns": 0,
        "allowed_tool_names": {"search_knowledge"},
        "scoped_modules": ["knowledge"],
        "expanded_modules": set(),
    }

    result = await manager_agent.call_tools_node(state)

    assert result["available_citations"] == {"c1"}


def test_envelope_summary_kem_citation_khi_la_knowledge_base():
    result = {
        "source": "knowledge_base", "items": [
            {"citationId": "c1", "sourceFile": "warranty.md", "heading": "Bảo hành", "content": "7 ngày"},
        ],
    }
    summary = manager_agent._envelope_summary(result)
    assert summary["citations"] == [
        {"citationId": "c1", "sourceFile": "warranty.md", "heading": "Bảo hành", "content": "7 ngày"},
    ]


def test_envelope_summary_khong_kem_citation_khi_khong_phai_knowledge_base():
    result = {"source": "sql", "items": [{"productId": 1}]}
    summary = manager_agent._envelope_summary(result)
    assert "citations" not in summary
