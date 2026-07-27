import json
import os
import re
from pathlib import Path

import pytest

MODEL_FALLBACK_RE = re.compile(r'os\.environ\.get\(\s*"MODEL"\s*(?:,\s*|\)\s*or\s*)"([^"]+)"')


def _appsettings_candidates(backend_root: Path) -> list[Path]:
    return sorted((backend_root / "WebAPI").glob("appsettings*.json"))


def _strip_jsonc_comments(raw: str) -> str:
    out = []
    in_string = escaped = False
    i, n = 0, len(raw)
    while i < n:
        ch = raw[i]
        if in_string:
            out.append(ch)
            if escaped:
                escaped = False
            elif ch == "\\":
                escaped = True
            elif ch == '"':
                in_string = False
            i += 1
        elif ch == '"':
            in_string = True
            out.append(ch)
            i += 1
        elif ch == "/" and i + 1 < n and raw[i + 1] == "/":
            while i < n and raw[i] != "\n":
                i += 1
        elif ch == "/" and i + 1 < n and raw[i + 1] == "*":
            i += 2
            while i + 1 < n and not (raw[i] == "*" and raw[i + 1] == "/"):
                i += 1
            i += 2
        else:
            out.append(ch)
            i += 1
    return re.sub(r",(\s*[}\]])", r"\1", "".join(out))


def _load_jsonc(path: Path) -> dict:
    cleaned = _strip_jsonc_comments(path.read_text(encoding="utf-8-sig"))
    try:
        return json.loads(cleaned)
    except json.JSONDecodeError as exc:  # pragma: no cover - chỉ chạy khi file hỏng
        raise AssertionError(f"Không parse được {path.name}: {exc}") from exc


def _read_appsettings_model(backend_root: Path) -> str:
    web_api = backend_root / "WebAPI"
    for name in ("appsettings.Template.json", "appsettings.json"):
        path = web_api / name
        if path.exists():
            break
    else:
        pytest.skip("Không tìm thấy appsettings.Template.json lẫn appsettings.json")

    return _load_jsonc(path)["AISetup"]["Model"]


def _read_factory_fallback(sidecar_root: Path) -> str:
    src = (sidecar_root / "services" / "llm_factory.py").read_text(encoding="utf-8")
    matches = MODEL_FALLBACK_RE.findall(src)
    assert matches, "Không tìm thấy fallback của MODEL trong llm_factory.py"
    return matches[-1]     


def test_ten_model_khop_giua_appsettings_va_sidecar(backend_root):
    sidecar_root = backend_root / "AISidecar"
    assert _read_appsettings_model(backend_root) == _read_factory_fallback(sidecar_root)


def test_moi_file_appsettings_khai_cung_mot_model(backend_root):
    models = {}
    for path in _appsettings_candidates(backend_root):
        if path.name == "appsettings.Development.json":
            continue
        section = _load_jsonc(path).get("AISetup")
        if section and "Model" in section:
            models[path.name] = section["Model"]

    assert models, "Không file appsettings nào khai AISetup:Model"
    assert len(set(models.values())) == 1, f"Tên model lệch nhau giữa các file: {models}"


def test_khong_hard_code_ten_model_ngoai_vi_tri_cho_phep(backend_root):
    allowed = set(_appsettings_candidates(backend_root))
    allowed.add(backend_root / "AISidecar" / "services" / "llm_factory.py")
    skip_dirs = {
        ".venv", "venv", "env", "node_modules", "obj", "bin", ".git",
        "TestResults", "docs", "tests", "__pycache__",
    }
    suffixes = {".py", ".cs", ".json", ".ts", ".vue", ".yml", ".yaml"}
    offenders = []
    for dirpath, dirnames, filenames in os.walk(backend_root):
        dirnames[:] = [d for d in dirnames if d not in skip_dirs]
        for filename in filenames:
            path = Path(dirpath) / filename
            if path.suffix not in suffixes or path in allowed:
                continue
            try:
                if "gemini-" in path.read_text(encoding="utf-8", errors="ignore"):
                    offenders.append(str(path.relative_to(backend_root)))
            except OSError:
                continue
    assert not offenders, f"Tên model bị hard-code ngoài vị trí cho phép: {offenders}"
