import json
import os
import re
from pathlib import Path

import pytest


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
    except json.JSONDecodeError as exc:
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


def test_ten_model_khop_giua_appsettings_va_settings(backend_root):
    from app.config import Settings

    appsettings_model = _read_appsettings_model(backend_root)
    default_model = Settings.model_fields["ai_model"].default

    assert appsettings_model == default_model, (
        f"appsettings.json có Model='{appsettings_model}' nhưng "
        f"app/config.py mặc định '{default_model}'"
    )


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


AISETUP_TO_SETTINGS = {
    "Provider": "ai_provider",
    "ApiEndpoint": "ai_api_endpoint",
    "ApiKey": "ai_api_key",
    "Model": "ai_model",
}

AISETUP_SKIP = {"LangSmithTracing", "LangSmithApiKey"}


def test_hop_dong_config_appsettings_vs_settings(backend_root):
    from app.config import Settings

    web_api = backend_root / "WebAPI"
    for name in ("appsettings.Template.json", "appsettings.json"):
        path = web_api / name
        if path.exists():
            break
    else:
        pytest.skip("Không tìm thấy appsettings")

    ai_setup = _load_jsonc(path).get("AISetup", {})
    settings_fields = set(Settings.model_fields.keys())

    missing_in_settings = []
    for key in ai_setup:
        if key in AISETUP_SKIP:
            continue
        expected_field = AISETUP_TO_SETTINGS.get(key)
        if expected_field and expected_field not in settings_fields:
            missing_in_settings.append(f"{key} → {expected_field}")

    assert not missing_in_settings, (
        f"AISetup keys thiếu field trong Settings: {missing_in_settings}"
    )


def test_khong_hard_code_ten_model_ngoai_vi_tri_cho_phep(backend_root):
    allowed = set(_appsettings_candidates(backend_root))
    allowed.add(backend_root / "AISidecar" / "app" / "config.py")
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
