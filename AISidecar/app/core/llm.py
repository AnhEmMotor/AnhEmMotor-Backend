from app.config import get_settings
from langchain_ollama import ChatOllama
from langchain_google_genai import ChatGoogleGenerativeAI


def get_llm(temperature=0.1, max_output_tokens: int | None = None):
    settings = get_settings()
    if settings.ai_provider.lower() == "apiendpoint":
        kwargs = {"model": settings.model, "temperature": temperature, "num_ctx": settings.ollama_num_ctx}
        if settings.ai_api_endpoint:
            kwargs["base_url"] = settings.ai_api_endpoint
        if max_output_tokens is not None:
            kwargs["num_predict"] = max_output_tokens
        return ChatOllama(**kwargs)
    else:
        if not settings.api_key:
            from langchain_core.language_models.fake import FakeListLLM
            return FakeListLLM(responses=['{"intent":"unknown"}'])
        kwargs = {
            "google_api_key": settings.api_key,
            "model": settings.model,
            "temperature": temperature,
        }
        if max_output_tokens is not None:
            kwargs["max_output_tokens"] = max_output_tokens
        return ChatGoogleGenerativeAI(**kwargs)
