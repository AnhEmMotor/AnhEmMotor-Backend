from app.config import get_settings
from langchain_ollama import ChatOllama
from langchain_google_genai import ChatGoogleGenerativeAI


def get_llm(temperature=0.1):
    settings = get_settings()
    if settings.ai_provider.lower() == "apiendpoint":
        kwargs = {"model": settings.model, "temperature": temperature, "num_ctx": settings.ollama_num_ctx}
        if settings.ai_api_endpoint:
            kwargs["base_url"] = settings.ai_api_endpoint
        return ChatOllama(**kwargs)
    else:
        if not settings.api_key:
            from langchain_core.language_models.fake import FakeListLLM
            return FakeListLLM(responses=['{"intent":"unknown"}'])
        return ChatGoogleGenerativeAI(
            google_api_key=settings.api_key,
            model=settings.model,
            temperature=temperature,
        )
