from datetime import datetime

from pydantic import BaseModel, ConfigDict


class ChatToolEnvelope(BaseModel):
    model_config = ConfigDict(extra="forbid")

    items: list[dict]
    totalCount: int
    truncated: bool
    asOf: datetime
    timezone: str
    source: str
    filtersApplied: dict[str, str]
    definition: str | None = None
    currency: str | None = None
    warnings: list[str] = []
