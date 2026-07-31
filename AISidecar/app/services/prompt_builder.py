import logging

from langchain_core.messages import SystemMessage, HumanMessage, AIMessage

from app.prompts.loader import render

logger = logging.getLogger(__name__)

FALLBACK_SYSTEM_PROMPT = (
    "Bạn là trợ lý AI của hệ thống quản lý AnhEmMotor. "
    "Hiện chưa lấy được thông tin người dùng, hãy trả lời ở mức chung "
    "và không đưa ra bất kỳ số liệu nội bộ nào."
)


def build_system_message(context: dict | None, server_date: str | None = None) -> SystemMessage:
    if not context:
        return SystemMessage(content=FALLBACK_SYSTEM_PROMPT)
    user = context.get("user") or {}
    full_name = user.get("fullName") or "(không rõ)"
    return SystemMessage(content=render(
        "system_manager_chat", full_name=full_name, server_date=server_date or "(không rõ)"))


def build_history_messages(context: dict | None, current_message: str) -> list:
    history = (context or {}).get("history") or []
    messages: list[HumanMessage | AIMessage] = []
    for item in history:
        role = (item.get("role") or "").lower()
        text = item.get("message") or ""
        if not text:
            continue
        if role == "user":
            messages.append(HumanMessage(content=text))
        elif role in ("ai", "assistant"):
            messages.append(AIMessage(content=text))
    if (messages
            and isinstance(messages[-1], HumanMessage)
            and messages[-1].content == current_message):
        messages.pop()
    return messages


def build_plan_prompt(user_request: str, existing_steps: list[dict]) -> str:
    locked = [s for s in existing_steps if s.get("editedByUser")]
    locked_text = "\n".join(
        f"- Bước {s['order']}: {s['title']} — {s['detail']}" for s in locked
    )
    return render(
        "system_plan_mode",
        request=user_request,
        locked_steps=locked_text or "(chưa có)",
        existing_count=len(existing_steps),
    )
