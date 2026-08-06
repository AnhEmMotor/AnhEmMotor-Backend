import hashlib
import json
import re

from pydantic import create_model

from app.core.llm import get_llm

FILLER_WORDS = ("cho tôi", "cho mình", "xem", "giúp", "vui lòng", "hãy", "ạ", "nhé")
SLOT_TYPES = {"date": str, "string": str, "integer": int}


def intent_hash(question: str, module: str) -> str:
    text = question.lower().strip()
    text = re.sub(r"[^\w\sàáâãèéêìíòóôõùúýăđĩũơưạảấầẩẫậắằẳẵặẹẻẽếềểễệ]", " ", text)
    text = re.sub(r"\s+", " ", text)

    text = re.sub(r"\b\d{1,2}[/-]\d{1,2}([/-]\d{2,4})?\b", "<ngay>", text)
    text = re.sub(r"\btháng\s+\d{1,2}\b", "<thang>", text)
    text = re.sub(r"\bquý\s+[1-4iv]+\b", "<quy>", text)
    text = re.sub(r"\bnăm\s+\d{4}\b", "<nam>", text)
    text = re.sub(r"\b\d+\b", "<so>", text)

    for filler in FILLER_WORDS:
        text = text.replace(filler, " ")
    text = re.sub(r"\s+", " ", text).strip()

    return hashlib.sha256(f"{module}|{text}".encode()).hexdigest()


def _build_slot_schema(slots: list[dict]):
    fields = {}
    for slot in slots:
        python_type = SLOT_TYPES.get(slot["type"], str)
        default = None if slot.get("optional") else ...
        fields[slot["name"]] = (python_type | None if slot.get("optional") else python_type, default)
    return create_model("SlotValues", **fields)


async def fill_slots(slots: list[dict], question: str, server_date: str) -> dict:
    if not slots:
        return {}
    schema = _build_slot_schema(slots)
    prompt = (
        "Trích xuất tham số cho câu hỏi sau, CHỈ trích xuất, KHÔNG lập lại kế hoạch.\n"
        f"Hôm nay (giờ Việt Nam): {server_date}\n"
        f"Câu hỏi: {question}\n"
        f"Danh sách slot cần điền: {json.dumps(slots, ensure_ascii=False)}\n"
    )
    llm = get_llm(temperature=0, max_output_tokens=200)
    structured = llm.with_structured_output(schema)
    result = await structured.ainvoke(prompt)
    return result.model_dump()


HARDCODED_DATA_PATTERNS = [
    re.compile(r"\bDH-\d{4}-\d+\b"),
    re.compile(r"\b\d{2,3}[.,]\d{3}[.,]\d{3}\b"),
    re.compile(r"\b(?:\+84|0)\d{9,10}\b"),
]


def contains_hardcoded_data(steps_template: list[dict]) -> bool:
    text = json.dumps(steps_template, ensure_ascii=False)
    return any(pattern.search(text) for pattern in HARDCODED_DATA_PATTERNS)


def render_steps(steps_template: list[dict], slot_values: dict) -> list[dict]:
    rendered = []
    for step in steps_template:
        title = step.get("title", "")
        detail = step.get("detail", "")
        for key, value in slot_values.items():
            placeholder = "{{" + key + "}}"
            title = title.replace(placeholder, str(value) if value is not None else "")
            detail = detail.replace(placeholder, str(value) if value is not None else "")
        rendered.append({**step, "title": title, "detail": detail})
    return rendered
