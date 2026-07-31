from pydantic import BaseModel


class ChatRequest(BaseModel):
    run_id: str
    session_id: str
    message: str
    server_date: str | None = None


class GenerateTitleRequest(BaseModel):
    message: str


class RevalidatePlanRequest(BaseModel):
    run_id: str
    expected_tools: list[str] = []
    fingerprint: str = ""
