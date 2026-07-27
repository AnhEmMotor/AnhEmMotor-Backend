import sys
from pathlib import Path
import pytest

SIDECAR_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SIDECAR_ROOT))

INTERNAL_SECRET = "test-internal-secret-abc123"

@pytest.fixture(autouse=True)
def _clean_env(monkeypatch):
    for key in ("API_KEY", "AI_PROVIDER", "MODEL", "AI_API_ENDPOINT", "BACKEND_URL"):
        monkeypatch.delenv(key, raising=False)
    monkeypatch.setenv("BACKEND_INTERNAL_SECRET", INTERNAL_SECRET)

@pytest.fixture
def internal_secret() -> str:
    return INTERNAL_SECRET

@pytest.fixture
def client():
    from fastapi.testclient import TestClient
    import main
    return TestClient(main.app)

@pytest.fixture
def backend_root() -> Path:
    return SIDECAR_ROOT.parent
