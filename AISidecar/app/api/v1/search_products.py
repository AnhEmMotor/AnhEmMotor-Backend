import logging
from fastapi import APIRouter, Depends
from langchain_core.prompts import PromptTemplate
from langchain_core.output_parsers import PydanticOutputParser
from app.api.deps import verify_internal_secret
from app.core.llm import get_llm
from app.prompts.loader import render
from app.schemas.search_products import SearchIntent

logger = logging.getLogger(__name__)

router = APIRouter()


def _get_structured_llm():
    llm = get_llm(temperature=0.1)
    try:
        return llm.with_structured_output(SearchIntent)
    except (NotImplementedError, AttributeError):
        return None


@router.post("/search")
async def search(request_data: dict, _: str = Depends(verify_internal_secret)):
    keyword = request_data.get("keyword", "")
    try:
        if not keyword.strip():
            return {"result": SearchIntent().model_dump(), "status": "success"}
        structured_llm = _get_structured_llm()
        if structured_llm is not None:
            prompt_text = render("search_intent", query=keyword)
            result = await structured_llm.ainvoke(prompt_text)
            return {"result": result.model_dump(), "status": "success"}
        parser = PydanticOutputParser(pydantic_object=SearchIntent)
        base_prompt = render("search_intent", query="{query}")
        template = PromptTemplate(
            template=base_prompt + "\n{format_instructions}",
            input_variables=["query"],
            partial_variables={"format_instructions": parser.get_format_instructions()},
        )
        chain = template | get_llm(temperature=0.1) | parser
        result = await chain.ainvoke({"query": keyword})
        return {"result": result.model_dump(), "status": "success"}
    except Exception:
        logger.exception("Error calling LLM for search")
        fallback = SearchIntent(keyword=keyword, intent="search").model_dump()
        return {"result": fallback, "status": "error"}
