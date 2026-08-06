from app.services import qdrant_client as qc


def _enable_rag(monkeypatch):
    monkeypatch.setenv("QDRANT_URL", "http://localhost:6333")


def test_index_products_goi_upsert(client, internal_secret, monkeypatch):
    _enable_rag(monkeypatch)
    calls = []

    async def fake_upsert(items):
        calls.append(items)

    monkeypatch.setattr(qc, "upsert_products", fake_upsert)

    resp = client.post(
        "/internal/index/products",
        json={"items": [{"productId": 1, "name": "SP 1"}]},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert resp.json() == {"indexed": 1}
    assert calls == [[{"productId": 1, "name": "SP 1"}]]


def test_index_products_delete_goi_delete(client, internal_secret, monkeypatch):
    _enable_rag(monkeypatch)
    calls = []

    async def fake_delete(product_ids):
        calls.append(product_ids)

    monkeypatch.setattr(qc, "delete_products", fake_delete)

    resp = client.post(
        "/internal/index/products/delete",
        json={"productIds": [1, 2]},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert resp.json() == {"deleted": 2}
    assert calls == [[1, 2]]


def test_index_knowledge_goi_index_knowledge(client, internal_secret, monkeypatch):
    _enable_rag(monkeypatch)
    calls = []

    async def fake_index_knowledge(documents):
        calls.append(documents)

    monkeypatch.setattr(qc, "index_knowledge", fake_index_knowledge)

    resp = client.post(
        "/internal/index/knowledge",
        json={"documents": [{"chunkId": "k1", "content": "..."}]},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert resp.json() == {"indexed": 1}


def test_rebuild_tu_choi_collection_khong_ho_tro(client, internal_secret, monkeypatch):
    _enable_rag(monkeypatch)
    resp = client.post(
        "/internal/index/rebuild",
        json={"collection": "knowledge_base", "items": []},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert "error" in resp.json()


def test_rebuild_goi_reindex_products(client, internal_secret, monkeypatch):
    _enable_rag(monkeypatch)

    async def fake_reindex(items, expected_count):
        return {"aliasSwitched": True, "stagingCount": expected_count, "expectedCount": expected_count}

    monkeypatch.setattr(qc, "reindex_products", fake_reindex)

    resp = client.post(
        "/internal/index/rebuild",
        json={"collection": "product_catalog", "items": [{"productId": 1}]},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert resp.json()["aliasSwitched"] is True


def test_index_products_bo_qua_khi_rag_tat(client, internal_secret, monkeypatch):
    monkeypatch.delenv("QDRANT_URL", raising=False)
    called = []
    monkeypatch.setattr(qc, "upsert_products", lambda items: called.append(items))

    resp = client.post(
        "/internal/index/products",
        json={"items": [{"productId": 1}]},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert resp.json() == {"skipped": "rag_disabled"}
    assert called == []


def test_index_knowledge_bo_qua_khi_rag_tat(client, internal_secret, monkeypatch):
    monkeypatch.delenv("QDRANT_URL", raising=False)
    called = []
    monkeypatch.setattr(qc, "index_knowledge", lambda documents: called.append(documents))

    resp = client.post(
        "/internal/index/knowledge",
        json={"documents": [{"chunkId": "k1"}]},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert resp.json() == {"skipped": "rag_disabled"}
    assert called == []


def test_rebuild_bo_qua_khi_rag_tat(client, internal_secret, monkeypatch):
    monkeypatch.delenv("QDRANT_URL", raising=False)

    resp = client.post(
        "/internal/index/rebuild",
        json={"collection": "product_catalog", "items": []},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert resp.json() == {"skipped": "rag_disabled"}


def test_delete_products_bo_qua_khi_rag_tat(client, internal_secret, monkeypatch):
    monkeypatch.delenv("QDRANT_URL", raising=False)
    called = []
    monkeypatch.setattr(qc, "delete_products", lambda product_ids: called.append(product_ids))

    resp = client.post(
        "/internal/index/products/delete",
        json={"productIds": [1]},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    assert resp.json() == {"skipped": "rag_disabled"}
    assert called == []
