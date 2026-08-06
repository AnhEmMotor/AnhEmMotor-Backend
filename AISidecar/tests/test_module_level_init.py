import importlib
import sys
from pathlib import Path

import pytest

SIDECAR_ROOT = Path(__file__).resolve().parents[1]
DEPS_SRC = SIDECAR_ROOT / "app" / "api" / "deps.py"


def test_import_app_khong_can_env(monkeypatch):
    for key in ("API_KEY", "BACKEND_URL", "BACKEND_INTERNAL_SECRET", "QDRANT_URL"):
        monkeypatch.delenv(key, raising=False)

    for name in [m for m in sys.modules if m.startswith("app.")]:
        sys.modules.pop(name, None)

    importlib.import_module("app.main")


def test_verify_internal_secret_dung_settings():
    src = DEPS_SRC.read_text(encoding="utf-8")
    assert "os.environ" not in src, \
        "verify_internal_secret phải dùng Settings thay vì os.environ (Stage 7)"


def test_main_chi_bind_loopback():
    main_src = (SIDECAR_ROOT / "main.py").read_text(encoding="utf-8")
    assert '"0.0.0.0"' not in main_src, "sidecar không được bind mọi interface"
    assert 'host="127.0.0.1"' in main_src


def test_sidecar_manager_truyen_dung_host_cho_uvicorn():
    manager = SIDECAR_ROOT.parent / "Infrastructure" / "Services" / "Ai" / "AiSidecarManager.cs"
    if not manager.exists():
        pytest.skip("Không tìm thấy AiSidecarManager.cs")
    src = manager.read_text(encoding="utf-8")
    assert "--host 0.0.0.0" not in src
    assert "--host 127.0.0.1" in src
