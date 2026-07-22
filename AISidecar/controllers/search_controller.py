import os
import json
from fastapi import APIRouter, Depends
from dependencies import verify_internal_token
from langchain_google_genai import ChatGoogleGenerativeAI
from langchain_core.prompts import PromptTemplate
from pydantic import BaseModel, Field
from langchain_core.output_parsers import PydanticOutputParser
from typing import List, Optional

router = APIRouter()

class SearchIntent(BaseModel):
	keyword: Optional[str] = Field(description="Tên sản phẩm cụ thể nếu có, ví dụ: SH 150i, Air Blade, mũ bảo hiểm", default="")
	brand: Optional[str] = Field(description="Tên thương hiệu nếu có, ví dụ: Honda, Yamaha, Shoei", default="")
	category: Optional[str] = Field(description="Danh mục chính, ví dụ: Xe máy, Phụ tùng, Phụ kiện", default="")
	vehicleType: Optional[str] = Field(description="Loại xe nếu có, ví dụ: Xe ga, Xe số, Xe côn tay", default="")
	priceMin: Optional[int] = Field(description="Giá thấp nhất (VNĐ) nếu có", default=0)
	priceMax: Optional[int] = Field(description="Giá cao nhất (VNĐ) nếu có", default=60000000)
	colors: List[str] = Field(description="Danh sách màu sắc nếu có, ví dụ: Đỏ, Đen, Xanh", default=[])
	intent: str = Field(description="Ý định người dùng (search, unknown)", default="search")

gemini_api_key = os.environ.get("GEMINI_API_KEY", "")
gemini_model_name = os.environ.get("GEMINI_MODEL", "gemini-3.5-flash")

if gemini_api_key:
	llm = ChatGoogleGenerativeAI(google_api_key=gemini_api_key, model=gemini_model_name, temperature=0.1)
else:
	from langchain_core.language_models.fake import FakeListLLM
	llm = FakeListLLM(responses=['{"intent":"unknown"}'])

parser = PydanticOutputParser(pydantic_object=SearchIntent)

prompt_template = PromptTemplate(
	template=(
		"Bạn là một trợ lý bán xe máy, phụ tùng và phụ kiện."
		" Hãy phân tích yêu cầu sau của khách hàng và trích xuất thông tin để tìm kiếm."
		"\n\nYêu cầu của khách: {query}\n{format_instructions}"
	),
	input_variables=["query"],
	partial_variables={"format_instructions": parser.get_format_instructions()},
)

chain = prompt_template | llm | parser

@router.post("/search")
def search(request_data: dict, token: str = Depends(verify_internal_token)):
	keyword = request_data.get("keyword", "")
	user_id = request_data.get("userId", None)

	try:
		if not keyword.strip():
			return {"result": SearchIntent().dict(), "status": "success"}

		result = chain.invoke({"query": keyword})
		return {"result": result.dict(), "status": "success"}
	except Exception as e:
		print(f"Error calling LLM: {e}")
		fallback = SearchIntent(keyword=keyword, intent="search").dict()
		return {"result": fallback, "status": "error"}
