from app.core.llm import get_llm

DEFAULT_MODEL = "gemini-3.5-flash"


def test_khong_co_api_key_tra_fake_llm(monkeypatch):
    monkeypatch.delenv("API_KEY", raising=False)
    llm = get_llm()
    assert type(llm).__name__ == "FakeListLLM"


def test_provider_apiendpoint_dung_chat_ollama(monkeypatch):
    monkeypatch.setenv("AI_PROVIDER", "apiendpoint")
    monkeypatch.setenv("AI_API_ENDPOINT", "http://localhost:11434")
    monkeypatch.setenv("MODEL", "qwen2.5:7b")
    llm = get_llm()
    assert type(llm).__name__ == "ChatOllama"
    assert llm.model == "qwen2.5:7b"
    assert "localhost:11434" in str(llm.base_url), "phải trỏ đúng endpoint từ env"


def test_provider_apiendpoint_khong_can_api_key(monkeypatch):
    monkeypatch.setenv("AI_PROVIDER", "apiendpoint")
    monkeypatch.setenv("AI_API_ENDPOINT", "http://localhost:11434")
    monkeypatch.delenv("API_KEY", raising=False)
    llm = get_llm()
    assert type(llm).__name__ == "ChatOllama"


def test_gemini_ton_trong_env_model(monkeypatch):
    monkeypatch.setenv("API_KEY", "fake-key-for-test")
    monkeypatch.setenv("MODEL", "gemini-3.5-flash")
    llm = get_llm()
    assert type(llm).__name__ == "ChatGoogleGenerativeAI"
    assert DEFAULT_MODEL in str(llm.model)


def test_env_model_rong_thi_dung_fallback(monkeypatch):
    monkeypatch.setenv("API_KEY", "fake-key-for-test")
    monkeypatch.setenv("MODEL", "")
    llm = get_llm()
    assert DEFAULT_MODEL in str(llm.model)


def test_temperature_duoc_truyen_dung(monkeypatch):
    monkeypatch.setenv("API_KEY", "fake-key-for-test")
    llm = get_llm(temperature=0.42)
    assert llm.temperature == 0.42
