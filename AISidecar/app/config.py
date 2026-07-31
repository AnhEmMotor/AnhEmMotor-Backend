from functools import lru_cache
from typing import Literal

from pydantic import field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")
    backend_url: str = "http://localhost:5000/api"
    backend_internal_secret: str = ""
    ai_provider: Literal["gemini", "apiendpoint"] = "gemini"
    api_key: str = ""
    ai_api_endpoint: str = ""
    model: str = "gemini-3.5-flash"
    ollama_num_ctx: int = 16384

    @field_validator("model", mode="before")
    @classmethod
    def _empty_model_uses_default(cls, v):
        return v or "gemini-3.5-flash"

    @field_validator("ai_provider", mode="before")
    @classmethod
    def _lower_ai_provider(cls, v):
        return v.lower() if isinstance(v, str) else v

    port: int = 8000
    request_timeout_seconds: float = 15.0

    tool_detail_level: Literal["Full", "Summary", "Minimal"] = "Full"

    qdrant_url: str = ""
    qdrant_api_key: str = ""
    embedding_model: str = "text-embedding-004"
    rag_enabled: bool = True

    postgres_url: str = ""

    plan_cache_enabled: bool = True

    tool_flags: dict[str, Literal["off", "shadow", "canary", "full"]] = {}

    expected_build_id: str = ""

    @property
    def backend_base(self) -> str:
        return self.backend_url.rstrip("/").removesuffix("/api")



@lru_cache
def get_settings() -> Settings:
    return Settings()
