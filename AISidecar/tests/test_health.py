from app.api.v1 import health


async def _fake_verify_tool_contract(_client):
    return {"stale_build": False}


async def test_health_store_chat_ready_true_khi_catalog_hop_le(monkeypatch):
    health._stale_cache["checked_at"] = 0.0
    monkeypatch.setattr(health, "verify_tool_contract", _fake_verify_tool_contract)

    result = await health.health()

    assert result["store_chat_ready"] is True


async def test_health_store_chat_ready_false_khi_catalog_loi(monkeypatch):
    health._stale_cache["checked_at"] = 0.0
    monkeypatch.setattr(health, "verify_tool_contract", _fake_verify_tool_contract)
    monkeypatch.setattr(health, "load_store_catalog", lambda: (_ for _ in ()).throw(FileNotFoundError()))

    result = await health.health()

    assert result["store_chat_ready"] is False
