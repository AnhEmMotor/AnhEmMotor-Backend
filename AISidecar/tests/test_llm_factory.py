from app.core.llm import get_llm

DEFAULT_MODEL = "gpt-4o-mini"


def test_khong_co_api_key_tra_fake_llm(monkeypatch):
    monkeypatch.delenv("AI_API_KEY", raising=False)
    llm = get_llm()
    assert type(llm).__name__ == "FakeListLLM"


def test_provider_apiendpoint_dung_chat_ollama(monkeypatch):
    monkeypatch.setenv("AI_PROVIDER", "apiendpoint")
    monkeypatch.setenv("AI_API_ENDPOINT", "http://localhost:11434")
    monkeypatch.setenv("AI_MODEL", "qwen2.5:7b")
    llm = get_llm()
    assert type(llm).__name__ == "ChatOllama"
    assert llm.model == "qwen2.5:7b"
    assert "localhost:11434" in str(llm.base_url), "phải trỏ đúng endpoint từ env"


def test_provider_apiendpoint_khong_can_api_key(monkeypatch):
    monkeypatch.setenv("AI_PROVIDER", "apiendpoint")
    monkeypatch.setenv("AI_API_ENDPOINT", "http://localhost:11434")
    monkeypatch.delenv("AI_API_KEY", raising=False)
    llm = get_llm()
    assert type(llm).__name__ == "ChatOllama"


def test_openai_ton_trong_env_model(monkeypatch):
    monkeypatch.setenv("AI_API_KEY", "fake-key-for-test")
    monkeypatch.setenv("AI_MODEL", "gpt-4o-mini")
    llm = get_llm()
    assert type(llm).__name__ == "ChatOpenAI"
    assert DEFAULT_MODEL in str(llm.model)


def test_env_model_rong_thi_dung_fallback(monkeypatch):
    monkeypatch.setenv("AI_API_KEY", "fake-key-for-test")
    monkeypatch.setenv("AI_MODEL", "")
    llm = get_llm()
    assert DEFAULT_MODEL in str(llm.model)


def test_temperature_duoc_truyen_dung(monkeypatch):
    monkeypatch.setenv("AI_API_KEY", "fake-key-for-test")
    llm = get_llm(temperature=0.42)
    assert llm.temperature == 0.42


def test_ollama_nhan_num_ctx_du_lon_cho_toan_bo_tool(monkeypatch):
    monkeypatch.setenv("AI_PROVIDER", "apiendpoint")
    monkeypatch.setenv("AI_API_ENDPOINT", "http://localhost:11434")
    llm = get_llm()
    assert type(llm).__name__ == "ChatOllama"
    assert llm.num_ctx >= 8192, (
        "context quá nhỏ sẽ vượt ngưỡng khi catalog có nhiều tool (mỗi tool ~1000+ ký tự "
        "description/args) — xem lỗi 'exceed_context_size_error' đã gặp thật với 'Phiếu nhập?'"
    )
