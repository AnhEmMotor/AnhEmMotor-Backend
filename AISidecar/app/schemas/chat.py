from pydantic import BaseModel


class ChatRequest(BaseModel):
    run_id: str
    session_id: str
    message: str


class GenerateTitleRequest(BaseModel):
    message: str
