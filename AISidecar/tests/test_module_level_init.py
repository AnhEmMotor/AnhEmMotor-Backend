import importlib
import sys
from pathlib import Path

import pytest

SIDECAR_ROOT = Path(__file__).resolve().parents[1]
SEARCH_CONTROLLER_SRC = SIDECAR_ROOT / "controllers" / "search_controller.py"
MAIN_SRC = SIDECAR_ROOT / "main.py"
DEPENDENCIES_SRC = SIDECAR_ROOT / "dependencies.py"


def _reimport_search_controller():
    name = "controllers.search_controller"
    saved = sys.modules.pop(name, None)
    try:
        return importlib.import_module(name)
    finally:
        if saved is not None:
            sys.modules[name] = saved
        else:
            sys.modules.pop(name, None)


def test_import_search_controller_khong_goi_get_llm(monkeypatch):
    calls = []
    monkeypatch.setattr(
        "services.llm_factory.get_llm",
        lambda *args, **kwargs: calls.append(1),
    )

    module = _reimport_search_controller()

    assert not calls, "get_llm không được gọi lúc import search_controller (mục 1.5)"
    assert not hasattr(module, "llm"), "không được giữ LLM ở module level"
    assert not hasattr(module, "chain"), "không được dựng chain ở module level"
    assert module.__dict__.get("_chain") is None, "cache chain phải còn rỗng sau khi import"


def test_search_dung_chain_dung_cache(monkeypatch):
    module = _reimport_search_controller()
    monkeypatch.setattr(module, "_chain", None)

    built = []
    monkeypatch.setattr(module, "get_llm", lambda *a, **k: built.append(1) or "fake-llm")
    monkeypatch.setattr(module, "prompt_template", _FakeRunnable())
    monkeypatch.setattr(module, "parser", "fake-parser")

    module._get_chain()
    module._get_chain()

    assert len(built) == 1, "get_llm chỉ được gọi ở lượt đầu tiên"


class _FakeRunnable:
    def __or__(self, other):
        return self


def test_khong_con_import_thua():
    content = SEARCH_CONTROLLER_SRC.read_text(encoding="utf-8")
    assert "from langchain_google_genai import" not in content, \
        "Xoá import thừa ChatGoogleGenerativeAI (mục 1.5)"


def test_dung_model_dump_thay_vi_dict():
    content = SEARCH_CONTROLLER_SRC.read_text(encoding="utf-8")
    assert ".dict()" not in content, "Đổi .dict() sang .model_dump() (mục 1.5)"


def test_verify_internal_secret_doc_env_trong_ham():
    src = DEPENDENCIES_SRC.read_text(encoding="utf-8")
    head, sep, body = src.partition("def verify_internal_secret")
    assert sep, "không tìm thấy verify_internal_secret"
    assert 'os.environ.get("BACKEND_INTERNAL_SECRET"' in body, \
        "verify_internal_secret phải đọc env trong hàm (mục 1.6.2)"


def test_main_chi_bind_loopback():
    src = MAIN_SRC.read_text(encoding="utf-8")
    assert '"0.0.0.0"' not in src, "sidecar không được bind mọi interface (mục 1.4)"
    assert 'host="127.0.0.1"' in src


def test_sidecar_manager_truyen_dung_host_cho_uvicorn():
    manager = SIDECAR_ROOT.parent / "Infrastructure" / "Services" / "Ai" / "AiSidecarManager.cs"
    if not manager.exists():
        pytest.skip("Không tìm thấy AiSidecarManager.cs")
    src = manager.read_text(encoding="utf-8")
    assert "--host 0.0.0.0" not in src
    assert "--host 127.0.0.1" in src
