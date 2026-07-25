import os

def get_llm(temperature=0.1):
    provider = os.environ.get("AI_PROVIDER", "Gemini")
    api_key = os.environ.get("API_KEY", "")
    
    if provider.lower() == "apiendpoint":
        from langchain_openai import ChatOpenAI
        endpoint = os.environ.get("AI_API_ENDPOINT", "")
        model_name = os.environ.get("MODEL", "gpt-3.5-turbo")
        # Ollama / Local provider might not require an API key
        if not api_key:
            api_key = "ollama"
            
        return ChatOpenAI(
            api_key=api_key,
            base_url=endpoint if endpoint else None,
            model=model_name,
            temperature=temperature
        )
    else:
        # Default to Gemini
        if not api_key:
            from langchain_core.language_models.fake import FakeListLLM
            return FakeListLLM(responses=['{"intent":"unknown"}'])
        from langchain_google_genai import ChatGoogleGenerativeAI
        model_name = os.environ.get("MODEL", "gemini-3.5-flash")
        return ChatGoogleGenerativeAI(google_api_key=api_key, model=model_name, temperature=temperature)
