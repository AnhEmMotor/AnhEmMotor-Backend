import re
from typing import Any

REDACT_FIELDS = {
    "password", "passwordhash", "token", "accesstoken", "refreshtoken",
    "apikey", "secret", "internalsecret", "connectionstring",
    "securitystamp", "concurrencystamp", "creditcard", "cardnumber", "cvv",
    "email", "phone", "phonenumber", "address", "identitycard",
    "citizenid", "fullname", "customername", "bankaccount",
}

SENSITIVE_PATTERNS = [
    (re.compile(r"\b[\w.+-]+@[\w-]+\.[\w.]+\b"), "[email]"),
    (re.compile(r"\b(?:\+84|0)\d{9,10}\b"), "[số điện thoại]"),
    (re.compile(r"\b\d{9,12}\b"), "[số định danh]"),
    (re.compile(r"\b[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\b"), "[token]"),
    (re.compile(r"(?i)\b(sk|lsv2|AIza)[-_a-z0-9]{16,}\b"), "[api key]"),
]

MAX_PREVIEW_CHARS = 500


def _scrub_text(text: str) -> str:
    for pattern, replacement in SENSITIVE_PATTERNS:
        text = pattern.sub(replacement, text)
    return text


def redact_value(key: str, value: Any) -> Any:
    normalized = key.lower().replace("_", "")

    if normalized in REDACT_FIELDS:
        return "***"
    if isinstance(value, str):
        return _scrub_text(value)
    if isinstance(value, dict):
        return redact_dict(value)
    if isinstance(value, list):
        return [redact_value(key, v) for v in value[:10]]
    return value


def redact_dict(data: dict) -> dict:
    return {k: redact_value(k, v) for k, v in data.items()}


def make_tool_preview(payload: dict) -> dict:
    safe = redact_dict(payload)
    text = str(safe)
    if len(text) > MAX_PREVIEW_CHARS:
        text = text[:MAX_PREVIEW_CHARS] + f"… (đã rút gọn, tổng {len(text)} ký tự)"
    return {"preview": text}
