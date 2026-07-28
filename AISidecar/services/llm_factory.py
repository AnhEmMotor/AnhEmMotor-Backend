import os
from langchain_ollama import ChatOllama
from langchain_google_genai import ChatGoogleGenerativeAI

def get_llm(temperature=0.1):
    provider = os.environ.get("AI_PROVIDER", "Gemini")
    api_key = os.environ.get("API_KEY", "")
    if provider.lower() == "apiendpoint":
        endpoint = os.environ.get("AI_API_ENDPOINT", "")
        model_name = os.environ.get("MODEL") or "qwen2.5:7b"
        kwargs = {"model": model_name, "temperature": temperature}
        if endpoint:
            kwargs["base_url"] = endpoint
        return ChatOllama(**kwargs)
    else:
        if not api_key:
            from langchain_core.language_models.fake import FakeListLLM
            return FakeListLLM(responses=['{"intent":"unknown"}'])
        model_name = os.environ.get("MODEL") or "gemini-3.5-flash"
        return ChatGoogleGenerativeAI(google_api_key=api_key, model=model_name, temperature=temperature)
