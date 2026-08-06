import logging

from app.core.errors import LlmError
from app.core.llm import get_llm
from app.tools.registry import DEFAULT_MODULES_ON_ROUTER_FAILURE

logger = logging.getLogger(__name__)

MAX_DIGEST_TURNS = 3
MAX_MSG_CHARS = 160
ROUTING_CONTEXT_TTL_SECONDS = 30 * 60

ANAPHORA = ("nó", "cái đó", "cái này", "vậy còn", "thế còn", "còn ", "cái kia", "đó")

MODULES = (
    "product", "inventory", "supplier", "sales", "contract", "customer",
    "marketing", "service", "warranty", "finance", "hr", "logistics",
    "admin", "knowledge", "none",
)

ROUTER_PROMPT = """Phân loại câu hỏi của người quản lý cửa hàng xe máy vào nhóm phù hợp.

Các nhóm:
- product: sản phẩm, giá bán, biến thể, danh mục, thương hiệu
- inventory: tồn kho, nhập kho, phiếu kho, sổ kho, yêu cầu mua hàng
- supplier: nhà cung cấp, giá nhập, báo giá từ nhà cung cấp
- sales: đơn hàng, doanh thu, hoá đơn bán, báo cáo bán hàng
- contract: hợp đồng bán, hợp đồng tài chính, hợp đồng nhà cung cấp
- customer: khách hàng, lead, chăm sóc khách, khách hàng thân thiết
- marketing: tin tức, banner, voucher, khuyến mãi
- service: sửa chữa, lịch hẹn, dịch vụ xưởng, xe của khách
- warranty: bảo hành, yêu cầu bảo hành, điều khoản bảo hành
- finance: công nợ, chi phí, thanh toán, lợi nhuận
- hr: nhân viên, KPI, hoa hồng, lương
- logistics: vận chuyển, giao hàng, phí ship
- admin: tổng quan hệ thống, cấu hình, người dùng và phân quyền
- knowledge: chính sách, quy trình, hướng dẫn nội bộ
- none: chào hỏi, tán gẫu, câu hỏi không cần dữ liệu

Chỉ trả về tên nhóm, tối đa 2 nhóm, cách nhau bởi dấu phẩy.

{digest}

Câu hỏi hiện tại: {query}

Nếu câu hỏi hiện tại là câu tiếp nối (dùng "nó", "cái đó", "vậy còn"...),
hãy dựa vào phần "Đang nói về" và "Nhóm tool lượt trước" để chọn nhóm."""


def build_routing_digest(history: list[dict], routing_ctx: dict) -> str:
    recent_questions = [
        item["message"][:MAX_MSG_CHARS]
        for item in history
        if (item.get("role") or "").lower() == "user"
    ][-MAX_DIGEST_TURNS:]

    parts = []
    if recent_questions:
        parts.append("Câu hỏi gần đây: " + " | ".join(recent_questions))

    entities = routing_ctx.get("entities") or {}
    if entities:
        described = ", ".join(f"{k}={v}" for k, v in entities.items())
        parts.append(f"Đang nói về: {described}")

    if last := routing_ctx.get("lastModules"):
        parts.append(f"Nhóm tool lượt trước: {', '.join(last)}")

    return "\n".join(parts)


ENTITY_FROM_ARGS = {
    "product_id": "product",
    "product_name": "product",
    "from_date": "period",
    "order_code": "orderCode",
    "order_id": "orderCode",
    "supplier_id": "supplier",
    "customer_id": "customer",
}


def extract_entities(tool_calls: list[dict]) -> dict:
    found = {}
    for call in tool_calls:
        for arg, value in (call.get("args") or {}).items():
            key = ENTITY_FROM_ARGS.get(arg)
            if key and value not in (None, "", []):
                found[key] = value
    return found


def is_follow_up(query: str, routing_ctx: dict) -> bool:
    if not routing_ctx.get("lastModules"):
        return False
    words = query.strip().split()
    if len(words) > 8:
        return False
    lowered = query.lower()
    return any(marker in lowered for marker in ANAPHORA)


async def route_question(query: str, digest: str) -> list[str]:
    llm = get_llm(temperature=0)
    try:
        response = await llm.ainvoke(ROUTER_PROMPT.format(digest=digest, query=query))
    except Exception as exc:
        raise LlmError(str(exc)) from exc
    text = response if isinstance(response, str) else getattr(response, "text", "")
    modules = [m.strip() for m in text.split(",") if m.strip() in MODULES]
    return modules[:2]


async def resolve_modules(query: str, routing_ctx: dict, history: list[dict]) -> list[str]:
    if is_follow_up(query, routing_ctx):
        logger.info("Fast path: tái dùng nhóm tool lượt trước")
        return routing_ctx["lastModules"]

    digest = build_routing_digest(history, routing_ctx)
    try:
        modules = await route_question(query, digest)
        return modules or (routing_ctx.get("lastModules") or DEFAULT_MODULES_ON_ROUTER_FAILURE)
    except (TimeoutError, LlmError) as e:
        logger.warning("Router lỗi: %s", e)
        return routing_ctx.get("lastModules") or DEFAULT_MODULES_ON_ROUTER_FAILURE


def expire_if_stale(routing_ctx: dict, now) -> dict:
    updated_at = routing_ctx.get("updatedAt")
    if not updated_at:
        return routing_ctx
    try:
        age = (_parse(now) - _parse(updated_at)).total_seconds()
    except (ValueError, TypeError):
        return routing_ctx
    if age > ROUTING_CONTEXT_TTL_SECONDS:
        return {}
    return routing_ctx


def _parse(value):
    from datetime import datetime
    if isinstance(value, str):
        return datetime.fromisoformat(value)
    return value
