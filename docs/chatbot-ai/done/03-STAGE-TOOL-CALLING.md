# Stage 3 — Tool Calling / Truy vấn dữ liệu thật

> Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 3–5 ngày · Phụ thuộc: **Stage 2**
> Mục tiêu: biến chatbot từ "chat chay" thành **trợ lý quản lý** truy vấn được dữ liệu thật.

Đây là Stage tạo giá trị lớn nhất của cả tính năng.

---

## 3.1. Nguyên tắc thiết kế

```
User: "Tháng này bán được bao nhiêu xe SH?"
   ↓
Sidecar (LangChain Agent)
   ↓ chọn tool: get_sales_summary(product_keyword="SH", from="2026-07-01", to="2026-07-31")
   ↓ HTTP POST /internal/chat/tools/sales-summary  (kèm JWT của user + X-Internal-Secret)
Backend .NET
   ↓ KIỂM TRA PERMISSION của user cho tool này  ← điểm chốt bảo mật
   ↓ query DB qua repository sẵn có
   ↓ trả JSON gọn
Sidecar → LLM diễn giải thành câu trả lời tiếng Việt
```

**3 nguyên tắc bất di bất dịch:**

1. **LLM không bao giờ chạm trực tiếp vào DB.** Không dùng SQL Agent, không truyền connection
   string cho Python. Mọi truy vấn đi qua endpoint .NET có kiểm tra quyền.
2. **Permission check nằm ở .NET, không ở prompt.** Prompt chỉ để AI "biết mà từ chối cho lịch sự";
   backend mới là nơi thực sự chặn. Prompt có thể bị inject, backend thì không.
3. **Tool trả dữ liệu đã tổng hợp, không trả bảng thô.** Trả 10 dòng số liệu, không trả 5000 record.

---

## 3.2. Tập tool khởi đầu (MVP)

Chọn 5–6 tool giá trị cao trước, không làm dàn trải.

| Tool | Mô tả | Permission yêu cầu | Endpoint |
|---|---|---|---|
| `search_products` | Tìm sản phẩm theo keyword/brand/category/giá | `Product.View` | `POST /internal/chat/tools/products/search` |
| `get_product_stock` | Tồn kho + biến thể của 1 sản phẩm | `Product.View` | `POST /internal/chat/tools/products/stock` |
| `get_order_status` | Tra cứu đơn hàng theo mã | `Order.View` | `POST /internal/chat/tools/orders/status` |
| `get_sales_summary` | Doanh thu / số đơn theo khoảng thời gian | `Analytics.View` | `POST /internal/chat/tools/analytics/sales` |
| `get_low_stock_products` | Danh sách sản phẩm sắp hết hàng | `Product.View` | `POST /internal/chat/tools/products/low-stock` |
| `get_top_selling` | Top N sản phẩm bán chạy | `Analytics.View` | `POST /internal/chat/tools/analytics/top-selling` |

> **Việc cần làm trước khi code:** mở bảng `Permissions` trong DB (hoặc seeder) và ghi lại **tên
> permission thật** của dự án vào bảng trên. Tên ở đây chỉ là placeholder.

---

## 3.3. Backend — Tool endpoints

### Cấu trúc thư mục đề xuất

```
WebAPI/Controllers/
  InternalChatController.cs           (đã có — giữ nguyên, chỉ lo context)
  InternalChatToolsController.cs      (mới)

Application/Features/ChatTools/
  Queries/
    SearchProductsForChat/
      SearchProductsForChatQuery.cs
      SearchProductsForChatQueryHandler.cs
      ChatProductDto.cs
    GetSalesSummaryForChat/
      ...
```

### Controller mẫu

```csharp
[Route("internal/chat/tools")]
[ApiController]
[Authorize]
[LocalhostOnly]
public class InternalChatToolsController(ISender sender) : ControllerBase
{
    [HttpPost("products/search")]
    [RequirePermission("Product.View")]        // dùng attribute permission sẵn có của dự án
    public async Task<IActionResult> SearchProducts(
        [FromBody] SearchProductsForChatRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new SearchProductsForChatQuery(request.Keyword, request.BrandId,
                                           request.CategoryId, request.Limit), ct);
        return Ok(result);
    }
}
```

> Kiểm tra tên attribute permission thật trong `WebAPI/Attributes/` trước khi viết.
> Dự án đã có `IPermissionReadRepository.HasAnyPermissionAsync` — cần bản check **theo tên
> permission cụ thể**, nếu chưa có thì bổ sung `HasPermissionAsync(userId, permissionName, ct)`.

### Quy tắc DTO trả về cho LLM

- Field tên rõ nghĩa tiếng Anh, snake hoặc camel nhất quán.
- **Giới hạn cứng số bản ghi** (mặc định 10, tối đa 25) — LLM không cần nhiều hơn.
- Format tiền tệ để nguyên số, thêm field `currency: "VND"`, để LLM tự format.
- Kèm `total_count` để AI biết còn bao nhiêu kết quả nữa.

```csharp
public record ChatProductDto(
    Guid Id, string Name, string? Brand, string? Category,
    decimal Price, int StockQuantity, string Currency = "VND");

public record ChatToolResult<T>(IReadOnlyList<T> Items, int TotalCount, bool Truncated);
```

---

## 3.4. Sidecar — LangChain Agent

### Bước 1 — Tool wrapper chung

Tạo `AISidecar/services/tool_client.py`:

```python
import os
import httpx

BACKEND_INTERNAL_SECRET = os.environ.get("BACKEND_INTERNAL_SECRET", "")


def _tools_base_url() -> str:
    raw = os.environ.get("BACKEND_URL", "http://localhost:5000/api")
    return raw.rstrip("/").replace("/api", "") + "/internal/chat/tools"


async def call_tool(path: str, payload: dict, auth_header: str) -> dict:
    """Gọi tool endpoint ở backend .NET.

    Trả dict có key "error" nếu thất bại — LLM sẽ đọc và diễn giải cho user.
    """
    url = f"{_tools_base_url()}/{path.lstrip('/')}"
    headers = {
        "Authorization": auth_header,
        "X-Internal-Secret": BACKEND_INTERNAL_SECRET,
    }
    try:
        async with httpx.AsyncClient(timeout=15.0) as client:
            resp = await client.post(url, json=payload, headers=headers)
            if resp.status_code == 403:
                return {"error": "forbidden",
                        "message": "Người dùng không có quyền truy cập dữ liệu này."}
            if resp.status_code != 200:
                return {"error": "backend_error", "status": resp.status_code}
            return resp.json()
    except httpx.HTTPError as e:
        return {"error": "connection_error", "message": str(e)}
```

### Bước 2 — Định nghĩa tool

Tạo `AISidecar/services/chat_tools.py`:

```python
from langchain_core.tools import StructuredTool
from pydantic import BaseModel, Field
from typing import Optional
from services.tool_client import call_tool


class SearchProductsInput(BaseModel):
    keyword: Optional[str] = Field(default=None, description="Từ khoá tên sản phẩm, ví dụ: SH 150i")
    brand: Optional[str] = Field(default=None, description="Tên thương hiệu, ví dụ: Honda")
    category: Optional[str] = Field(default=None, description="Danh mục, ví dụ: Xe máy, Phụ tùng")
    limit: int = Field(default=10, description="Số kết quả tối đa, mặc định 10")


class SalesSummaryInput(BaseModel):
    from_date: str = Field(description="Ngày bắt đầu, định dạng YYYY-MM-DD")
    to_date: str = Field(description="Ngày kết thúc, định dạng YYYY-MM-DD")
    group_by: str = Field(default="day", description="Nhóm theo: day | week | month")


def build_tools(auth_header: str) -> list:
    """Tạo danh sách tool đã gắn sẵn auth của user hiện tại.

    auth_header được closure vào từng tool nên LLM không thể tự đổi danh tính.
    """

    async def _search_products(**kwargs):
        return await call_tool("products/search", kwargs, auth_header)

    async def _sales_summary(**kwargs):
        return await call_tool("analytics/sales", kwargs, auth_header)

    return [
        StructuredTool.from_function(
            coroutine=_search_products,
            name="search_products",
            description=(
                "Tìm kiếm sản phẩm (xe máy, phụ tùng, phụ kiện) trong hệ thống. "
                "Dùng khi người dùng hỏi về sản phẩm, giá, tồn kho, hoặc muốn tìm hàng."
            ),
            args_schema=SearchProductsInput,
        ),
        StructuredTool.from_function(
            coroutine=_sales_summary,
            name="get_sales_summary",
            description=(
                "Lấy tổng hợp doanh thu và số đơn hàng trong một khoảng thời gian. "
                "Dùng khi người dùng hỏi về doanh thu, doanh số, kết quả kinh doanh."
            ),
            args_schema=SalesSummaryInput,
        ),
        # ... các tool còn lại
    ]
```

> **Chất lượng `description` quyết định độ chính xác của agent.** Viết như hướng dẫn cho người mới:
> nói rõ *khi nào dùng* và *khi nào không dùng*, kèm ví dụ câu hỏi.

### Bước 3 — Agent + streaming

```python
from langgraph.prebuilt import create_react_agent

async def stream_agent(messages, tools, llm):
    agent = create_react_agent(llm, tools)
    async for event in agent.astream_events({"messages": messages}, version="v2"):
        kind = event["event"]
        if kind == "on_chat_model_stream":
            chunk = event["data"]["chunk"]
            if chunk.content:
                yield chunk.content
        elif kind == "on_tool_start":
            # Tín hiệu để FE hiện "Đang tra cứu dữ liệu..."
            yield f"\n<tool-start>{event['name']}</tool-start>\n"
        elif kind == "on_tool_end":
            yield "\n<tool-end></tool-end>\n"
```

**Cần thêm vào `requirements.txt`:** `langgraph`, `langchain-core` (đã có gián tiếp).

> **Đã chốt: LangGraph** (không dùng `AgentExecutor` cũ của `langchain`).
>
> **Lộ trình hai bước — cả hai đều là LangGraph, không phải hai lựa chọn loại trừ:**
>
> | Giai đoạn | Dùng gì | Vì sao |
> |---|---|---|
> | **Stage 3** (ở đây) | `langgraph.prebuilt.create_react_agent` | Chạy được ngay với ~10 dòng, đủ để chứng minh hạ tầng tool |
> | **Từ Stage 9** | `StateGraph` tự dựng node | Cần node riêng cho steering (9.5), plan (10.6), tool scoping (20.7) — prebuilt không chèn được |
>
> `create_react_agent` bên trong **cũng là** một `StateGraph` biên dịch sẵn, nên chuyển đổi là
> mở rộng chứ không viết lại. Giữ nguyên `AgentState`, `astream_events`, và checkpointer.
>
> **Đừng bỏ qua bước prebuilt để nhảy thẳng lên `StateGraph`** — Stage 3 chỉ cần chứng minh
> tool calling chạy thông ba lớp; dựng graph tay lúc này là tối ưu hoá sớm.

### Bước 4 — Chuyển đổi protocol stream

Hiện backend đọc raw `text/plain` theo chunk 32 ký tự. Khi có tool call, cần phân biệt
"text trả lời" với "tín hiệu tool" → **đổi sang Server-Sent Events dạng JSON lines**:

```
{"type":"text","content":"Doanh thu tháng 7 là "}
{"type":"tool_start","name":"get_sales_summary"}
{"type":"tool_end","name":"get_sales_summary"}
{"type":"text","content":"1.2 tỷ đồng."}
{"type":"done"}
```

Ảnh hưởng dây chuyền — phải sửa cả 3 lớp:
1. `AISidecar/controllers/manager_chat_controller.py` — yield JSON lines.
2. `StreamManagerChatMessageCommandHandler.cs` — đọc theo **dòng** (`reader.ReadLineAsync()`)
   thay vì buffer 32 ký tự; chỉ nối `type == "text"` vào `fullReply` để lưu DB; forward nguyên
   dòng JSON qua SignalR.
3. `ChatDrawer.vue` — parse JSON mỗi chunk, render text và hiện indicator khi `tool_start`.

---

## 3.5. Lộ trình triển khai gợi ý

Chia nhỏ để không nghẽn:

| Bước | Nội dung | Kết quả kiểm chứng |
|---|---|---|
| 3.a | Đổi protocol stream sang JSON lines (chưa có tool) | Chat vẫn hoạt động y như cũ |
| 3.b | Làm 1 tool duy nhất: `search_products` (E2E) | Hỏi "còn xe SH không?" → AI trả số liệu thật |
| 3.c | Thêm permission check + test case bị từ chối | User không quyền → AI từ chối, log 403 |
| 3.d | Thêm 4–5 tool còn lại | Hỏi doanh thu, đơn hàng, tồn kho đều đúng |
| 3.e | Tinh chỉnh description + system prompt | Agent chọn đúng tool ≥ 90% trên bộ câu hỏi mẫu |

---

## 3.6. Rủi ro & cách giảm thiểu

| Rủi ro | Giảm thiểu |
|---|---|
| Agent gọi tool lặp vô hạn | Đặt `recursion_limit` cho agent (ví dụ 8 bước) |
| Latency tăng (nhiều round-trip LLM) | Stream `tool_start` để user thấy tiến trình; cache kết quả tool trong 1 lượt |
| LLM bịa số khi tool lỗi | Trả `{"error": ...}` rõ ràng + system prompt cấm bịa; test case tool trả 403 |
| Prompt injection qua dữ liệu sản phẩm (tên sản phẩm chứa chỉ thị) | Xử lý ở Stage 5 |
| Chi phí token tăng | Giới hạn `limit` bản ghi; dùng model rẻ (flash) cho tool selection |

---

## Definition of Done — Stage 3

- [ ] Bảng tên permission thật đã được điền vào mục 3.2.
- [ ] Protocol stream JSON lines chạy thông cả 3 lớp.
- [ ] Tối thiểu 5 tool hoạt động E2E.
- [ ] Mỗi tool endpoint có `[LocalhostOnly]` + permission check độc lập với prompt.
- [ ] Test: user không có `Analytics.View` hỏi doanh thu → nhận lời từ chối, **không có số liệu nào**.
- [ ] Agent có giới hạn số bước, không treo.
- [ ] Bộ 20 câu hỏi mẫu → agent chọn đúng tool ≥ 90%.
