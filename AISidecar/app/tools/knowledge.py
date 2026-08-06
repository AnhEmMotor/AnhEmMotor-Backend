import asyncio

from langchain_core.tools import StructuredTool
from pydantic import BaseModel, Field

from app.config import get_settings
from app.services import qdrant_client as qc
from app.services.backend_client import BackendClient

KNOWLEDGE_TOOL_NAMES = {"semantic_product_search", "search_knowledge"}


class SemanticProductSearchInput(BaseModel):
    query: str = Field(description=(
        "Mô tả nhu cầu bằng ngôn ngữ tự nhiên, ví dụ: "
        "'xe ga tiết kiệm xăng cho nữ', 'nhớt cho xe côn tay 150cc'"
    ))
    max_price: int | None = Field(default=None, description="Giá tối đa (VNĐ)")
    in_stock_only: bool = Field(default=True, description="Chỉ lấy hàng còn tồn")
    limit: int = Field(default=8, description="Số kết quả, tối đa 15")


class KnowledgeSearchInput(BaseModel):
    query: str = Field(description="Câu hỏi về chính sách, bảo hành, đổi trả, quy trình nội bộ")
    limit: int = Field(default=5)


def rag_enabled() -> bool:
    settings = get_settings()
    return bool(settings.qdrant_url) and settings.rag_enabled


def _make_semantic_product_search(backend_client: BackendClient):
    async def semantic_product_search(query: str, max_price: int | None = None,
                                       in_stock_only: bool = True, limit: int = 8) -> dict:
        hits = await qc.search_products(query, max_price=max_price, in_stock_only=in_stock_only, limit=limit)
        if not hits:
            return {"items": [], "totalCount": 0, "truncated": False, "source": "semantic"}

        details, stocks = await asyncio.gather(
            asyncio.gather(*(
                backend_client.call_tool("products/detail", {"product_id": int(h["productId"])})
                for h in hits
            ), return_exceptions=True),
            asyncio.gather(*(
                backend_client.call_tool("products/stock", {"product_id": int(h["productId"])})
                for h in hits
            ), return_exceptions=True),
        )

        items = []
        for hit, detail, stock in zip(hits, details, stocks):
            if isinstance(detail, BaseException) or not detail.get("items"):
                continue
            item = dict(detail["items"][0])
            if not isinstance(stock, BaseException):
                item["stockItems"] = stock.get("items")
            item["semanticScore"] = hit["score"]
            items.append(item)

        return {
            "items": items, "totalCount": len(items), "truncated": False,
            "source": "semantic",
        }

    return semantic_product_search


def _make_search_knowledge():
    async def search_knowledge(query: str, limit: int = 5) -> dict:
        chunks = await qc.search_knowledge(query, limit=limit)
        return {"items": chunks, "totalCount": len(chunks), "truncated": False, "source": "knowledge_base"}

    return search_knowledge


def build_knowledge_tools(backend_client: BackendClient,
                           allowed_names: set[str] | None = None) -> list[StructuredTool]:
    if not rag_enabled():
        return []

    tools = [
        StructuredTool.from_function(
            coroutine=_make_semantic_product_search(backend_client),
            name="semantic_product_search",
            description=(
                "Tìm sản phẩm theo MÔ TẢ, NHU CẦU hoặc ĐẶC ĐIỂM khi người dùng không nêu tên "
                "chính xác. Ví dụ: 'xe ga tiết kiệm xăng', 'đồ bảo hộ đi mưa'. "
                "KHÔNG dùng tool này khi người dùng đã nêu tên/mã sản phẩm cụ thể — "
                "khi đó hãy dùng search_products."
            ),
            args_schema=SemanticProductSearchInput,
        ),
        StructuredTool.from_function(
            coroutine=_make_search_knowledge(),
            name="search_knowledge",
            description=(
                "Tra cứu chính sách, bảo hành, đổi trả, quy trình nội bộ từ tài liệu công ty. "
                "Kết quả kèm citationId — PHẢI gắn mã trích dẫn dạng [c1] ngay sau câu dùng "
                "thông tin này, KHÔNG nêu tên tài liệu nếu không có mã tương ứng."
            ),
            args_schema=KnowledgeSearchInput,
        ),
    ]
    if allowed_names is not None:
        tools = [t for t in tools if t.name in allowed_names]
    return tools
