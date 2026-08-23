from app.config import get_settings
from langchain_ollama import ChatOllama
from langchain_openai import ChatOpenAI


def get_llm(temperature=0.1, max_output_tokens: int | None = None):
    settings = get_settings()
    if settings.ai_provider.lower() == "apiendpoint":
        kwargs = {"model": settings.ai_model, "temperature": temperature, "num_ctx": settings.ollama_num_ctx}
        if settings.ai_api_endpoint:
            kwargs["base_url"] = settings.ai_api_endpoint
        if max_output_tokens is not None:
            kwargs["num_predict"] = max_output_tokens
        return ChatOllama(**kwargs)
    else:
        if not settings.ai_api_key:
            from langchain_core.language_models.fake import FakeListLLM
            return FakeListLLM(responses=['{"intent":"unknown"}'])
        kwargs = {
            "api_key": settings.ai_api_key,
            "model": settings.ai_model,
            "temperature": temperature,
        }
        if settings.ai_api_endpoint:
            kwargs["base_url"] = settings.ai_api_endpoint
        if max_output_tokens is not None:
            kwargs["max_tokens"] = max_output_tokens
        return ChatOpenAI(**kwargs)
