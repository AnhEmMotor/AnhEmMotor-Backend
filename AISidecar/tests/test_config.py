import pytest
from app.config import Settings


def test_settings_doc_dung_env(monkeypatch):
    monkeypatch.setenv("BACKEND_URL", "http://example.com/api")
    monkeypatch.setenv("AI_API_KEY", "test-key")
    monkeypatch.setenv("PORT", "9999")
    s = Settings()
    assert s.backend_url == "http://example.com/api"
    assert s.ai_api_key == "test-key"
    assert s.port == 9999


def test_backend_base_bo_hau_to_api():
    s = Settings(backend_url="http://localhost:5000/api")
    assert s.backend_base == "http://localhost:5000"


def test_backend_base_bo_slash_thua():
    s = Settings(backend_url="http://localhost:5000/api/")
    assert s.backend_base == "http://localhost:5000"




def test_ai_provider_validate():
    with pytest.raises(Exception):
        Settings(ai_provider="invalid_provider")


def test_default_values():
    s = Settings()
    assert s.ai_model == "gemini-3.5-flash"
    assert s.port == 8000
    assert s.rag_enabled is True
    assert s.plan_cache_enabled is True
    assert s.tool_flags == {}
