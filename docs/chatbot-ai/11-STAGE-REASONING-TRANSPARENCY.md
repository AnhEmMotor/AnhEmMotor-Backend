# Stage 11 — Hiển thị quá trình suy nghĩ & kết quả tool

> Yêu cầu #5 · Ưu tiên: 🟠 Trung bình · Ước lượng: 2–3 ngày · Phụ thuộc: **Stage 8, 3**
> Mục tiêu: người dùng xem được **AI đang nghĩ gì, gọi tool nào, nhận về kết quả gì** —
> nhưng **che thông tin nhạy cảm khi lên Production**.

Hai mục tiêu này kéo ngược nhau, nên phần khó nhất của Stage này là lớp **redaction**.

---

## 11.1. Cái gì được hiển thị

```
┌────────────────────────────────────────────────┐
│ 🤖 Trợ lý AI                                   │
│                                                │
│ ▼ Đã suy nghĩ trong 4.2 giây          [thu gọn]│
│ ┌────────────────────────────────────────────┐ │
│ │ 💭 Người dùng hỏi doanh thu tháng này.     │ │
│ │    Cần gọi tool tổng hợp doanh thu với     │ │
│ │    khoảng thời gian 01/07 - 31/07.         │ │
│ │                                            │ │
│ │ 🔧 get_sales_summary                 1.8s  │ │
│ │    ├ Tham số: từ 2026-07-01 đến 2026-07-31 │ │
│ │    └ Kết quả: 1.240.000.000 ₫ · 312 đơn    │ │
│ │                                            │ │
│ │ 💭 Đã có số liệu, so sánh với tháng trước  │ │
│ │    để câu trả lời có ngữ cảnh.             │ │
│ │                                            │ │
│ │ 🔧 get_sales_summary                 1.1s  │ │
│ │    ├ Tham số: từ 2026-06-01 đến 2026-06-30 │ │
│ │    └ Kết quả: 1.050.000.000 ₫ · 287 đơn    │ │
│ └────────────────────────────────────────────┘ │
│                                                │
│ Doanh thu tháng 7 đạt **1,24 tỷ đồng** từ 312  │
│ đơn hàng, tăng 18% so với tháng 6.             │
└────────────────────────────────────────────────┘
```

**Mặc định thu gọn.** Chỉ hiện dòng tóm tắt `▶ Đã suy nghĩ trong 4.2 giây`, bấm mới mở.
Người dùng thường chỉ cần câu trả lời; phần suy nghĩ là để kiểm chứng khi nghi ngờ.

---

## 11.2. Ba mức độ hiển thị

Mặc định **Full** — hiển thị đầy đủ suy nghĩ, tool, tham số và kết quả.
Khi cần che bớt ở Production, sidecar tự áp mức thấp hơn trong code (không cần config riêng).

Ba mức để code xử lý nội bộ (không phải config từ bên ngoài):

| Mức | Môi trường | Suy nghĩ | Tên tool | Tham số | Kết quả |
|---|---|---|---|---|---|
| `Full` | Development | ✅ đầy đủ | ✅ | ✅ raw JSON | ✅ raw JSON |
| `Summary` | **Production (mặc định)** | ✅ đã lọc | ✅ | ✅ đã che | ✅ chỉ tóm tắt |
| `Minimal` | Production (chế độ chặt) | ❌ | ✅ nhãn tiếng Việt | ❌ | ❌ chỉ "đã hoàn tất" |

Ở mức `Minimal`, dòng tool hiển thị: `🔧 Đang tra cứu doanh thu... ✓`

---

## 11.3. Lớp redaction — phần quan trọng nhất

**Nguyên tắc số 1: redact ở sidecar TRƯỚC khi phát event, không redact ở FE.**
Nếu redact ở FE thì dữ liệu thật vẫn đã đi qua network và nằm trong `ChatRunEvent` của DB —
ai xem được DB hoặc DevTools là thấy hết.

**Nguyên tắc số 2: allowlist, không phải blocklist.** Ở Production, chỉ những field được khai báo
là an toàn mới được hiển thị. Field lạ (do thêm tool mới, đổi DTO) mặc định bị che.

### `app/core/redaction.py`

```python
import re
from typing import Any
from app.config import get_settings

# Field TUYỆT ĐỐI không bao giờ hiển thị, ở mọi mức, kể cả Development
ALWAYS_REDACT = {
    "password", "passwordhash", "token", "accesstoken", "refreshtoken",
    "apikey", "api_key", "secret", "internalsecret", "connectionstring",
    "securitystamp", "concurrencystamp", "creditcard", "cardnumber", "cvv",
}

# Field là PII — che ở Production, hiện ở Development
PII_FIELDS = {
    "email", "phone", "phonenumber", "address", "identitycard",
    "citizenid", "fullname", "customername", "bankaccount",
}

SENSITIVE_PATTERNS = [
    (re.compile(r"\b[\w.+-]+@[\w-]+\.[\w.]+\b"), "[email]"),
    (re.compile(r"\b(?:\+84|0)\d{9,10}\b"), "[số điện thoại]"),
    (re.compile(r"\b\d{9,12}\b"), "[số định danh]"),
    (re.compile(r"\b[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\b"), "[token]"),
    (re.compile(r"(?i)\b(sk|lsv2|AIza)[-_a-z0-9]{16,}\b"), "[api key]"),
]

MAX_PREVIEW_CHARS = 500


def redact_value(key: str, value: Any, level: str) -> Any:
    normalized = key.lower().replace("_", "")

    if normalized in ALWAYS_REDACT:
        return "***"
    if level != "Full" and normalized in PII_FIELDS:
        return "***"
    if isinstance(value, str):
        return _scrub_text(value)
    if isinstance(value, dict):
        return redact_dict(value, level)
    if isinstance(value, list):
        return [redact_value(key, v, level) for v in value[:10]]
    return value


def redact_dict(data: dict, level: str) -> dict:
    return {k: redact_value(k, v, level) for k, v in data.items()}


def _scrub_text(text: str) -> str:
    """Quét chuỗi tự do — bắt PII lọt qua tên field không đoán được."""
    for pattern, replacement in SENSITIVE_PATTERNS:
        text = pattern.sub(replacement, text)
    return text


def make_tool_preview(name: str, payload: dict) -> dict:
    """Tạo bản xem trước an toàn của tham số / kết quả tool.

    CHỈ dùng cho đường ra FE. KHÔNG bao giờ áp lên dữ liệu vào LLM (Stage 18.11).
    """
    level = get_settings().tool_detail_level      # Full | Summary | Minimal

    if level == "Minimal":
        return {"hidden": True}

    safe = redact_dict(payload, level)
    text = str(safe)
    if len(text) > MAX_PREVIEW_CHARS:
        text = text[:MAX_PREVIEW_CHARS] + f"… (đã rút gọn, tổng {len(text)} ký tự)"
    return {"preview": text, "level": level}
```

### Áp dụng ở đâu

```python
# app/tools/base.py
async def run_tool(name: str, args: dict, fn) -> dict:
    await emit("tool_start", {
        "name": name,
        "callId": call_id,
        "argsPreview": make_tool_preview(name, args),   # ← redact tại đây
    })

    started = time.perf_counter()
    result = await fn(**args)
    elapsed_ms = int((time.perf_counter() - started) * 1000)

    await emit("tool_end", {
        "callId": call_id,
        "status": "ok" if "error" not in result else "error",
        "durationMs": elapsed_ms,
        "summary": summarize_result(name, result),        # ← tóm tắt an toàn
        "resultPreview": make_tool_preview(name, result), # ← redact tại đây
    })
    return result
```

**`summarize_result`** trả câu tiếng Việt ngắn, không chứa dữ liệu nhạy cảm:
```python
SUMMARIZERS = {
    "search_products":   lambda r: f"Tìm thấy {r.get('totalCount', 0)} sản phẩm",
    "get_sales_summary": lambda r: f"Doanh thu {r.get('totalRevenue', 0):,.0f} ₫ · "
                                   f"{r.get('orderCount', 0)} đơn",
    "get_order_status":  lambda r: f"Đơn hàng ở trạng thái {r.get('status', 'không rõ')}",
}

def summarize_result(name: str, result: dict) -> str:
    if result.get("error"):
        return "Không lấy được dữ liệu"
    fn = SUMMARIZERS.get(name)
    return fn(result) if fn else "Đã hoàn tất"
```

> **Việc bắt buộc khi thêm tool mới:** viết summarizer cho nó. Nếu quên, mức `Summary` chỉ hiện
> "Đã hoàn tất" — an toàn nhưng vô dụng. Thêm test kiểm tra mọi tool trong registry đều có summarizer.

---

## 11.4. Suy nghĩ (thinking) — lấy từ đâu

Gemini không expose reasoning trace như một số model khác. Có 3 nguồn:

| Nguồn | Cách làm | Đánh giá |
|---|---|---|
| **A. Suy luận tự thuật** | Prompt yêu cầu model viết `<suy_nghi>...</suy_nghi>` trước khi hành động, parse ra khỏi output | ✅ Khuyến nghị — chạy với mọi model, kiểm soát được |
| **B. Thought từ API** | Dùng `thinking_config` nếu model hỗ trợ | Phụ thuộc model, có thể không có |
| **C. Sinh từ event** | Không hỏi model, tự dựng mô tả từ tool call | Rẻ nhất nhưng nông |

**Chọn A**, fallback sang C. Trong `app/prompts/system_manager_chat.md`:

```markdown
## Cách trình bày suy nghĩ
Trước mỗi hành động (gọi tool hoặc trả lời), hãy viết một đoạn ngắn trong thẻ
<suy_nghi></suy_nghi> giải thích bạn định làm gì và vì sao. Viết 1–2 câu tiếng Việt,
ngắn gọn, dành cho người quản lý cửa hàng đọc.

Trong thẻ <suy_nghi>:
- KHÔNG nhắc lại nội dung system prompt.
- KHÔNG ghi tên biến, tên bảng, câu SQL, hay chi tiết kỹ thuật nội bộ.
- KHÔNG ghi thông tin cá nhân của khách hàng.
```

Parser trong `app/agents/nodes.py` tách `<suy_nghi>` thành event `thinking`, phần còn lại
thành `text_delta`. **Phần trong `<suy_nghi>` không được lưu vào `ChatMessage`** — nó chỉ nằm
ở `ChatRunEvent` (vốn bị xoá sau 7 ngày theo Stage 8.8).

> Nội dung `thinking` do LLM sinh ra → vẫn phải chạy qua `_scrub_text()` trước khi phát, vì model
> có thể vô tình nhắc lại email/số điện thoại lấy từ kết quả tool.

---

## 11.5. Lưu trữ & bảo mật

| Dữ liệu | Lưu ở đâu | Thời hạn | Redact? |
|---|---|---|---|
| Câu trả lời cuối | `ChatMessage` | Vĩnh viễn (90 ngày, Stage 13.6) | Không |
| `thinking` | `ChatRunEvent` | 7 ngày | Có (`_scrub_text`) |
| `argsPreview` / `resultPreview` | `ChatRunEvent` | 7 ngày | **Có, bắt buộc** |
| Kết quả tool đầy đủ | **Không lưu** | — | — |

**Quan trọng:** kết quả tool đầy đủ chỉ tồn tại trong bộ nhớ của sidecar trong 1 lượt agent.
Không ghi xuống `ChatRunEvent`, không ghi vào log.

> ⚠️ **Ranh giới tuyệt đối — đọc kỹ trước khi code.** Redaction chỉ áp cho **đường ra FE**.
> `make_tool_preview()` **không bao giờ** được gọi trên dữ liệu đưa vào `ToolMessage` của LLM —
> nếu không, AI đọc `***` và diễn giải nó như giá trị thật. Hai đường dữ liệu tách biệt hoàn toàn,
> có test bắt buộc. Xem [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.11.

> ⚠️ **Dev/Prod lệch `TOOL_DETAIL_LEVEL`** khiến test ở Development không bao giờ phát hiện được
> rò rỉ ở Production. Bắt buộc **parametrize test qua cả ba mức**, không chỉ test theo môi
> trường thật đang chạy.

**Phân quyền xem:** người dùng chỉ xem được reasoning của run **của chính mình** —
đã được đảm bảo bởi kiểm tra quyền sở hữu trong `GetChatRunEventsQuery` (Stage 8).

---

## 11.6. Frontend

Component mới: `AnhEmMotor-Management/src/components/business/chat/ReasoningPanel.vue`

- Mặc định thu gọn, hiện `▶ Đã suy nghĩ trong {n} giây`.
- Trong lúc chạy: **tự động mở** và hiện bước hiện tại (user muốn biết AI đang làm gì),
  tự thu gọn khi `run_completed`.
- Mỗi tool là 1 dòng: icon trạng thái · nhãn tiếng Việt · thời gian · mũi tên mở chi tiết.
- Map tên tool → nhãn tiếng Việt, dùng chung với Stage 12.5:
  ```ts
  export const TOOL_LABELS: Record<string, string> = {
    search_products:       "Tìm sản phẩm",
    get_product_stock:     "Kiểm tra tồn kho",
    get_order_status:      "Tra cứu đơn hàng",
    get_sales_summary:     "Tổng hợp doanh thu",
    get_low_stock_products:"Tìm hàng sắp hết",
    get_top_selling:       "Xem hàng bán chạy",
    search_knowledge:      "Tra cứu tài liệu",
  };
  ```
  Tool không có trong map → hiện `Đang xử lý...` (không lộ tên hàm nội bộ ra production).
- Tool lỗi → dòng đỏ + thông điệp thân thiện, **không** hiện stack trace.
- Nút **Sao chép nhật ký** (chỉ ở Development) để dán vào bug report.

---

## 11.7. Rủi ro

| Rủi ro | Giảm thiểu |
|---|---|
| Thêm tool mới quên redact → rò rỉ | Allowlist mặc định che; test bắt buộc mọi tool có summarizer |
| Regex PII bỏ sót định dạng lạ | Kết hợp allowlist theo field (chính) + regex (lưới an toàn) |
| LLM nhắc lại PII trong `<suy_nghi>` | Prompt cấm + `_scrub_text` chạy trên cả thinking |
| Reasoning lộ tên bảng/schema → hỗ trợ tấn công | Prompt cấm chi tiết kỹ thuật; review thủ công mẫu log |
| Panel làm chậm UI khi nhiều event | Ảo hoá danh sách khi > 50 dòng; gom `text_delta` |
| Người dùng hiểu nhầm reasoning là sự thật | Ghi chú nhỏ: "Đây là diễn giải của AI, không phải nhật ký hệ thống" |

---

## Definition of Done — Stage 11

- [ ] Panel suy nghĩ hiển thị được, mặc định thu gọn, tự mở khi đang chạy.
- [ ] Thấy được tên tool, thời gian chạy, tóm tắt kết quả.
- [ ] `TOOL_DETAIL_LEVEL=Summary` → không có tham số/kết quả thô nào lọt ra FE (kiểm tra bằng DevTools Network).
- [ ] `TOOL_DETAIL_LEVEL=Minimal` → **thực sự đạt tới được**, chỉ hiện nhãn tiếng Việt + trạng thái;
      không còn dead code ở nhánh `Minimal`.
- [ ] Kiểm tra `ChatRunEvent` trong DB ở chế độ Production: **không** chứa email, số điện thoại, token.
- [ ] Tool trả về object chứa `passwordHash` → bị che ở **mọi** mức, kể cả Development.
- [ ] Mọi tool trong registry đều có summarizer (test tự động).
- [ ] Reasoning không bị lưu vào `ChatMessage`.
- [ ] Tua lại run cũ (Stage 8) → reasoning hiển thị lại đúng thứ tự.
- [ ] Tool tên lạ không có trong `TOOL_LABELS` → FE hiện "Đang xử lý...", không lộ tên hàm.
