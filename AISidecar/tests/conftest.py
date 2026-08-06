import sys
from pathlib import Path
import pytest

SIDECAR_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SIDECAR_ROOT))

INTERNAL_SECRET = "test-internal-secret-abc123"

@pytest.fixture(autouse=True)
def _clean_env(monkeypatch):
    for key in ("AI_API_KEY", "AI_PROVIDER", "AI_MODEL", "AI_API_ENDPOINT", "BACKEND_URL"):
        monkeypatch.delenv(key, raising=False)
    monkeypatch.setenv("BACKEND_INTERNAL_SECRET", INTERNAL_SECRET)

@pytest.fixture(autouse=True)
def _clear_settings_cache():
    from app.config import get_settings
    get_settings.cache_clear()
    yield
    get_settings.cache_clear()

@pytest.fixture
def internal_secret() -> str:
    return INTERNAL_SECRET

@pytest.fixture
def client():
    from fastapi.testclient import TestClient
    from app.main import app
    return TestClient(app)

@pytest.fixture
def backend_root() -> Path:
    return SIDECAR_ROOT.parent
