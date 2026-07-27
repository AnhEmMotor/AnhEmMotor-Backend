# Stage 20 — Chọn tool động theo ngữ cảnh

> Ưu tiên: 🔴 Cao · Ước lượng: 3–4 ngày
> Phụ thuộc cứng: **Stage 9** (ranh giới bước), **Stage 13** (router + trần).
> Mục **20.6** (scope theo bước plan) cần **Stage 10** — bổ sung khi Plan Mode xong.
> Nên làm **cùng lúc** với Stage 13 — hai Stage này là hai nửa của một cơ chế.

Stage 13.3b đặt trần 20 tool và chọn module một lần ở **đầu run**. Đủ cho câu hỏi độc lập,
nhưng sai với ba tình huống thực tế:

| Tình huống | Vì sao 13.3b không đủ |
|---|---|
| *"Còn màu đen không?"* (lượt 2) | Câu này **tự nó vô nghĩa** — router không biết đang nói về sản phẩm nào |
| User steering giữa run (Stage 9) | Nhu cầu tool đổi **sau khi** scope đã chốt |
| Plan được duyệt / bị sửa (Stage 10) | Tool cần thiết do **plan** quyết định, không phải câu hỏi gốc |

> **Luận điểm của Stage này:** chọn tool là **quyết định lặp lại ở mỗi bước**, không phải
> một lần ở đầu run.

---

## 20.1. Phễu 4 lớp

```
71 tool
  │
  ├─ Lớp 0: Lọc quyền (Stage 13.2)                    tĩnh trong 1 run
  │         Admin: 71 · Thủ kho: 13 · Marketing: 8
  │
  ├─ Lớp 1: Giải nghĩa ý định (20.2, 20.3)            MỖI LƯỢT — cần lịch sử
  │         → tập module
  │
  ├─ Lớp 2: Tập ứng viên (20.4, 20.5)                 MỖI BƯỚC — cần plan
  │         router ∪ plan-bước-hiện-tại ∪ pinned
  │
  └─ Lớp 3: Trần cứng + ưu tiên tất định (13.3b)      ≤ 20
```

Bốn lớp thay đổi ở bốn nhịp khác nhau — đó là lý do không thể tính một lần:

| Lớp | Nhịp thay đổi | Nguồn dữ liệu |
|---|---|---|
| 0 | Mỗi run (hoặc khi thu hồi quyền — 17.7) | `permissions` từ context |
| 1 | Mỗi lượt người dùng | câu hỏi + **routing digest** |
| 2 | **Mỗi bước agent** | plan hiện tại + steering |
| 3 | Mỗi lần dựng scope | luật tĩnh |

---

## 20.2. Lớp 1 — Routing digest, không phải toàn bộ lịch sử

**Sai lầm dễ mắc:** đưa cả lịch sử vào prompt router cho "chính xác hơn".
Thực tế ngược lại — router chậm hơn **và** kém hơn, vì câu trả lời dài của AI làm loãng tín hiệu.

**Cách làm:** dựng một **digest nén** chỉ chứa thứ router cần.

```python
MAX_DIGEST_TURNS = 3
MAX_MSG_CHARS = 160


def build_routing_digest(history: list[dict], routing_ctx: dict) -> str:
    """Digest gọn cho router: câu hỏi gần đây + thực thể đã nhắc.

    KHÔNG đưa câu trả lời của AI vào — chúng dài và làm loãng tín hiệu.
    """
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

    if last := routing_ctx.get("last_modules"):
        parts.append(f"Nhóm tool lượt trước: {', '.join(last)}")

    return "\n".join(parts)
```

Prompt router (Stage 13.3) nhận thêm digest:

```
{digest}

Câu hỏi hiện tại: {query}

Nếu câu hỏi hiện tại là câu tiếp nối (dùng "nó", "cái đó", "vậy còn"...),
hãy dựa vào phần "Đang nói về" và "Nhóm tool lượt trước" để chọn nhóm.
```

Chi phí: digest thường < 200 token, cố định bất kể lịch sử dài bao nhiêu.

---

## 20.3. Routing context — bộ nhớ thực thể giữa các lượt

Đây là thứ làm *"còn màu đen không?"* hoạt động.

### Data model

Thêm cột vào `ChatSession`:
```csharp
/// <summary>Ngữ cảnh định tuyến tool giữa các lượt, JSON. Nhỏ, ghi lại sau mỗi run.</summary>
[Column("RoutingContext", TypeName = "nvarchar(max)")]
public string RoutingContext { get; set; } = "{}";
```

Nội dung:
```jsonc
{
  "entities": {
    "product": "SH 150i ABS",
    "period": "2026-07",
    "orderCode": null
  },
  "lastModules": ["product", "inventory"],
  "updatedAt": "2026-07-26T09:15:00+07:00",
  "turnCount": 4
}
```

**Migration** — tạo cho **cả** MySQL và PostgreSQL:
```powershell
./add-migration.ps1 AddChatSessionRoutingContext
```

> **Vì sao ở `ChatSession` chứ không phải `ChatRun`:** nó phải sống **qua** các run — đó chính là
> mục đích. `ChatRun` chết sau mỗi lượt.

### Cập nhật sau mỗi run

Trích thực thể từ **tham số tool đã gọi**, không phải bằng LLM — rẻ và chính xác hơn:

```python
ENTITY_FROM_ARGS = {
    "product_id": "product",
    "product_name": "product",
    "from_date": "period",
    "order_code": "orderCode",
    "supplier_id": "supplier",
    "customer_id": "customer",
}


def extract_entities(tool_calls: list[dict]) -> dict:
    """Lấy thực thể từ tham số tool thực tế đã gọi trong run."""
    found = {}
    for call in tool_calls:
        for arg, value in (call.get("args") or {}).items():
            if key := ENTITY_FROM_ARGS.get(arg):
                if value not in (None, "", []):
                    found[key] = value       # lượt sau ghi đè lượt trước
    return found
```

### Hết hiệu lực

| Điều kiện | Hành động |
|---|---|
| Câu hỏi mới có thực thể **cùng loại nhưng khác giá trị** | Ghi đè thực thể đó |
| Câu hỏi mới đổi hẳn chủ đề (router chọn module không giao với `lastModules`) | Xoá `entities`, giữ `lastModules` |
| Không dùng > 30 phút | Xoá toàn bộ |
| Steering chế độ `interrupt` (Stage 9) | Xoá thực thể liên quan tới phần bị đính chính |

Mốc 30 phút quan trọng: user quay lại session cũ sau 2 giờ thì *"còn màu đen không?"* gần như
chắc chắn **không** nói về sản phẩm của 2 giờ trước. Giữ context cũ tệ hơn là không có.

---

## 20.4. Fast path cho câu tiếp nối — bỏ qua router

Câu tiếp nối chiếm phần lớn lượt chat và **không cần gọi router**:

```python
ANAPHORA = ("nó", "cái đó", "cái này", "vậy còn", "thế còn", "còn ", "cái kia", "đó")

def is_follow_up(query: str, routing_ctx: dict) -> bool:
    """Câu tiếp nối: ngắn, có từ chỉ định, và session đã có ngữ cảnh."""
    if not routing_ctx.get("lastModules"):
        return False
    words = query.strip().split()
    if len(words) > 8:
        return False
    lowered = query.lower()
    return any(marker in lowered for marker in ANAPHORA)


async def resolve_modules(query: str, routing_ctx: dict, history: list) -> list[str]:
    if is_follow_up(query, routing_ctx):
        logger.info("Fast path: tái dùng nhóm tool lượt trước")
        return routing_ctx["lastModules"]          # 0 token, 0ms

    digest = build_routing_digest(history, routing_ctx)
    try:
        return (await route_question(query, digest))[:2]     # trần 2 nhóm
    except (TimeoutError, LlmError) as e:
        logger.warning("Router lỗi: %s", e)
        # Fail-safe: ưu tiên nhóm lượt trước, rồi mới đến mặc định tĩnh
        return routing_ctx.get("lastModules") or DEFAULT_MODULES_ON_ROUTER_FAILURE
```

Hai lợi ích cùng lúc: **chính xác hơn** (tái dùng nhóm đã đúng ở lượt trước) và **rẻ hơn**
(bỏ hẳn một lời gọi LLM). Đây là bổ sung cho fast path ở Stage 14.2a — chỗ đó bỏ qua *agent*,
chỗ này bỏ qua *router*.

> **Rủi ro:** câu ngắn nhưng đổi chủ đề — *"còn nợ nhà cung cấp?"* có từ "còn" nhưng thuộc
> `finance`. Xử lý: nếu fast path dẫn tới `tool_not_found` hoặc agent gọi
> `request_more_tools`, tự nạp module đúng (17.3) — mất 1 vòng, không mất câu trả lời.
> Đo tỉ lệ này (20.8); nếu > 10% thì siết `ANAPHORA` lại.

---

## 20.5. Lớp 2 — Tập ứng viên: router ∪ plan ∪ pinned

```python
PINNED_TOOLS = frozenset({"search_knowledge"})     # giữ TỐI THIỂU


def build_tool_scope(state: AgentState) -> list[ToolSpec]:
    """Dựng tập tool cho BƯỚC hiện tại. Gọi lại ở mỗi ranh giới bước."""
    allowed = state["permitted_tools"]              # Lớp 0, tĩnh trong run

    names: set[str] = set(PINNED_TOOLS)

    # (a) Từ plan — chỉ tool của bước ĐANG chạy, không phải cả plan
    if step := state.get("current_plan_step"):
        names |= set(step.get("expectedTools") or [])

    # (b) Từ router — module của lượt hiện tại
    names |= {t.name for t in allowed if t.module in state["scoped_modules"]}

    # (c) Module tự nạp thêm (17.3)
    names |= {t.name for t in allowed if t.module in state["expanded_modules"]}

    scoped = [t for t in allowed if t.name in names]
    return apply_hard_cap(scoped, state)            # Lớp 3 — 13.3b
```

### `PINNED_TOOLS` phải cực nhỏ

Mỗi tool always-on tiêu một suất trong trần **của mọi run**. Chỉ `search_knowledge` xứng đáng
(tri thức nội bộ liên quan tới hầu hết câu hỏi, và không thuộc module nào).

> **Bỏ `request_more_tools` khỏi pinned.** Stage 17.3 đã tự nạp module khi model gọi tên tool có
> thật nhưng chưa nạp — tool tường minh này thành dư thừa, lại cho model thêm một cách tiêu vòng
> vô ích. Xoá khỏi Stage 13.3.

---

## 20.6. Plan là lịch trình tool — điểm mấu chốt

Khi plan được duyệt (Stage 10.5), mỗi bước đã khai `expectedTools`. Nghĩa là **plan chính là
bảng phân bổ tool được tính trước**.

```
Plan đã duyệt:
  Bước 1: Lấy DS sản phẩm tồn kho thấp    → expectedTools: [get_low_stock_products]
  Bước 2: Tính giá trị tồn theo danh mục  → expectedTools: [get_inventory_report]
  Bước 3: So sánh cùng kỳ năm ngoái       → expectedTools: [get_sales_summary]
  Bước 4: Tổng hợp báo cáo                → expectedTools: []

Tool nạp khi chạy:
  Bước 1: 1 + 1 pinned = 2 tool     ← không phải 20
  Bước 2: 1 + 1 = 2
  Bước 3: 1 + 1 = 2
  Bước 4: 0 + 1 = 1
```

**Plan Mode làm việc chọn tool dễ đi, không khó thêm.** Với 2 tool trong ngữ cảnh, khả năng chọn
sai gần như bằng không, và token đầu vào giảm mạnh.

### Khi plan bị sửa

Vì scope **suy ra** từ plan chứ không cache, sửa plan là scope tự theo:

| Thay đổi plan | Scope bước đó |
|---|---|
| User sửa nội dung bước | Tính lại `expectedTools` từ nội dung mới |
| User thêm bước | Bước mới có scope riêng khi tới lượt |
| User xoá bước (`skipped`) | Bỏ qua, không nạp gì |
| Tool trong bước đã bị gỡ (17.8) | Bước → `invalid`, chờ duyệt lại |

**`expectedTools` của bước user vừa sửa phải được tính lại**, vì user viết bằng tiếng Việt, không
khai tool. Dùng `FastModel`, prompt ngắn, chỉ cho chọn trong danh sách tool user có quyền:

```python
async def infer_step_tools(step_text: str, allowed: list[ToolSpec]) -> list[str]:
    """Suy ra tool cho một bước plan do user viết tay."""
    catalog = "\n".join(f"- {t.name}: {t.short_desc}" for t in allowed)
    result = await fast_llm.with_structured_output(StepToolsSchema).ainvoke(
        render("infer_step_tools", step=step_text, catalog=catalog)
    )
    # Lọc lại: model có thể bịa tên
    return [n for n in result.tools if any(t.name == n for t in allowed)][:3]
```

Trần 3 tool/bước: bước cần hơn 3 tool là bước quá lớn, nên tách.

---

## 20.7. Khi nào tính lại scope

Gắn vào **đúng ranh giới bước đã có ở Stage 9** (`absorb_steering`) — không tạo hook mới:

```python
async def absorb_steering_node(state: AgentState) -> dict:
    pending = await backend.pull_pending_steering(state["run_id"])
    updates = {}

    if pending:
        # Ánh xạ trực tiếp sang chế độ steering của Stage 9.1
        modes = {item["mode"] for item in pending}
        if "interrupt" in modes:
            # THAY ĐỔI hướng → tính lại scope từ đầu
            updates["scoped_modules"] = await resolve_modules(
                pending[-1]["content"], state["routing_context"], state["history"])
            updates["expanded_modules"] = set()
        else:
            # THÊM thông tin → mở rộng scope, giữ nguyên cái đang có
            extra = await resolve_modules(
                pending[-1]["content"], state["routing_context"], state["history"])
            updates["scoped_modules"] = list(
                dict.fromkeys([*state["scoped_modules"], *extra]))[:3]

        updates["messages"] = build_steering_messages(pending)

    updates["tool_scope"] = build_tool_scope({**state, **updates})
    return updates
```

**Đây chính là câu trả lời cho "có lúc nó thêm, có lúc thay đổi plan":** hai chế độ đã định nghĩa ở
Stage 9.1 ánh xạ thẳng sang hai phép toán trên scope.

| Chế độ steering | Phép toán trên scope | Vì sao |
|---|---|---|
| `queue` (thêm) | **Hợp** — union, trần 3 module | User bổ sung, việc cũ vẫn cần làm |
| `interrupt` (đổi) | **Thay** — tính lại từ đầu | Việc cũ không còn phù hợp |
| `restart` | Run mới, scope mới hoàn toàn | — |

Union cho phép **3 module** (không phải 2) vì steering bổ sung là ý định thứ hai hợp lệ.
Trần 20 tool ở Lớp 3 vẫn giữ — 3 module có thể vượt 20, và lúc đó cắt theo ưu tiên: plan-bước-hiện-tại
trước, rồi module gốc, rồi module bổ sung.

### Bảng đầy đủ các mốc tính lại

| Sự kiện | Tính lại | Ghi chú |
|---|---|---|
| Bắt đầu run | Lớp 0,1,2,3 | Toàn bộ |
| Ranh giới bước, không có steering | Lớp 2,3 | Rẻ, không gọi LLM |
| Steering `queue` | Lớp 1 (union), 2, 3 | 1 lời gọi router |
| Steering `interrupt` | Lớp 1 (thay), 2, 3 | 1 lời gọi router |
| Plan được duyệt | Lớp 2,3 | Scope theo bước 1 |
| Chuyển sang bước plan tiếp theo | Lớp 2,3 | Thường 2–3 tool |
| Plan bị sửa | Lớp 2,3 + `infer_step_tools` | Chỉ bước bị sửa |
| `tool_not_found` / module thiếu (17.3) | Lớp 2,3 | Nạp thêm module, tối đa 1 lần |
| Permission bị thu hồi (17.7) | Lớp 0,2,3 | Run mới mới áp |

---

## 20.8. Lịch sử dài — hai ngân sách tách biệt

Đây là chỗ dễ nhầm nhất: **lịch sử cho định tuyến** và **lịch sử cho trả lời** là hai thứ khác nhau,
với hai ngân sách khác nhau.

| | Lịch sử để **định tuyến** | Lịch sử để **trả lời** |
|---|---|---|
| Mục đích | Biết đang nói về cái gì | Hiểu và trả lời đúng |
| Nội dung | Chỉ câu hỏi user + thực thể | Cả hỏi và đáp |
| Kích thước | **Cố định < 200 token** | 20 tin / tóm tắt (Stage 2.4, 14.6) |
| Tăng theo độ dài session? | **Không** | Có, tới khi tóm tắt |
| Ở đâu | `build_routing_digest` (20.2) | `sanitize_history` (17.4) |

**Hệ quả quan trọng: session dài 200 tin nhắn không làm việc chọn tool tệ đi.** Digest vẫn 3 câu
hỏi cuối + thực thể. Chỉ phần trả lời chịu ảnh hưởng, và đã có tóm tắt xử lý.

### Tương tác với tóm tắt (Stage 18.9)

Stage 18.9 quy định tóm tắt **không chứa số liệu**. Nhưng routing context **cần** thực thể
(mã sản phẩm, kỳ báo cáo). Không mâu thuẫn — chúng là hai kho khác nhau:

- `ChatSession.Summary` → chủ đề và quyết định, **không số liệu**, dùng để trả lời
- `ChatSession.RoutingContext` → thực thể có cấu trúc, **không phải văn bản tự do**, dùng để định tuyến

Thực thể trong `RoutingContext` là *cái đang nói tới*, không phải *giá trị số liệu* — nên
`product: "SH 150i"` là hợp lệ, còn `revenue: 1240000000` thì **không** được lưu.

> **Test bắt buộc:** `RoutingContext` không chứa field số liệu tiền tệ / số lượng.
> Nếu lọt, ta vừa tái tạo đúng lỗi C4 (Stage 18.8 — trả lời bằng số cũ) qua cửa sau.

---

## 20.9. Nạp tool vào Qdrant — khi nào nên, khi nào không

Đây là lựa chọn thay thế cho Lớp 1+2: embed mô tả tool, tìm ngữ nghĩa theo câu hỏi.
Bạn đã lưu ý đây là phạm trù khác — tôi tách riêng và **không** đưa vào đường chính.

| | LLM router theo module (đang chọn) | Qdrant tool retrieval |
|---|---|---|
| Độ trễ | ~300ms (1 lời gọi `FastModel`) | ~15ms + embedding câu hỏi |
| Chi phí | ~80 token | 1 embedding (cache được) |
| Câu tiếp nối | Tốt — hiểu digest | **Kém** — vector của "còn màu đen không" gần như vô nghĩa |
| Tool gần giống nhau | Tốt — `KHÔNG DÙNG KHI` trong mô tả có tác dụng | **Kém** — `list_orders` vs `get_order_statistics` rất gần nhau về vector |
| Số tool | Tốt tới ~150 | Tốt ở mọi quy mô |
| Giải thích được | Có — module là khái niệm người đọc hiểu | Khó — chỉ có điểm cosine |

**Khuyến nghị: giữ router làm chính, dùng Qdrant ở hai chỗ hẹp.**

1. **Thay fail-safe tĩnh khi router lỗi.** Hiện 13.3b dùng `["product", "sales"]` cố định.
   Qdrant cho lựa chọn tốt hơn mà không cần LLM:
   ```python
   except (TimeoutError, LlmError):
       return routing_ctx.get("lastModules") or await qdrant_suggest_modules(query, top_k=2)
   ```
2. **Mở rộng ứng viên khi `tool_not_found`.** Thay vì `difflib.get_close_matches` (khớp chuỗi,
   Stage 17.3), tìm theo ngữ nghĩa — gợi ý đúng hơn nhiều khi model bịa tên tool nhưng đúng ý định.

**Chưa nên làm nó thành đường chính ở quy mô 71 tool.** Hai ô đánh dấu "Kém" ở trên đều là điểm
yếu chí tử: câu tiếp nối chiếm phần lớn lượt chat, và cặp tool gần giống nhau chính là nguồn lỗi
tôi đã ghi ở Stage 13.9. Xem xét lại khi vượt ~150 tool.

Nếu triển khai: collection `tool_catalog`, payload gồm `name`, `module`, `required_permissions`
(để **lọc trước khi** search, không lọc sau), `is_write`, `status` — cùng quy tắc Stage 12.7.

---

## 20.10. Đo lường

| Chỉ số | Mục tiêu | Ý nghĩa nếu lệch |
|---|---|---|
| Số tool trung vị nạp/bước | **≤ 5** | Cao = router quá rộng hoặc plan không khai `expectedTools` |
| Số tool p95 nạp/bước | ≤ 20 | Vượt = trần không được enforce |
| Tỉ lệ fast path (câu tiếp nối) | 25–45% | Thấp = `ANAPHORA` quá hẹp; cao = đang bỏ qua router sai |
| Tỉ lệ fast path dẫn tới nạp thêm module | **≤ 10%** | Cao = fast path đoán sai, cần siết |
| Tỉ lệ `tool_not_found` | ≤ 2% | Cao = scope quá hẹp hoặc mô tả tool kém |
| Tỉ lệ module tự nạp (17.3) | ≤ 15% | Cao = router chọn sai nhóm thường xuyên |
| Token digest trung bình | < 200 | Vượt = digest đang phình theo lịch sử |

Ghi cùng chỗ với chỉ số Stage 14.1. **Số tool trung vị/bước là chỉ số quan trọng nhất** — nó đo
trực tiếp việc phễu có hoạt động không.

---

## 20.11. Test

`AISidecar/tests/test_tool_scoping.py`

```python
def test_follow_up_tai_dung_module_luot_truoc():
    ctx = {"lastModules": ["product"], "entities": {"product": "SH 150i"}}
    assert is_follow_up("còn màu đen không?", ctx)
    assert await resolve_modules("còn màu đen không?", ctx, []) == ["product"]


def test_khong_co_ngu_canh_thi_khong_fast_path():
    """Câu tiếp nối ở lượt đầu tiên phải gọi router, không được đoán."""
    assert not is_follow_up("còn màu đen không?", {})


def test_cau_dai_khong_fast_path():
    ctx = {"lastModules": ["product"]}
    long_q = "cho tôi xem doanh thu tháng này so với cùng kỳ năm ngoái theo từng danh mục"
    assert not is_follow_up(long_q, ctx)


def test_digest_khong_phinh_theo_lich_su():
    """Lịch sử 200 tin nhắn vẫn cho digest cố định."""
    history = [{"role": "User", "message": f"câu hỏi số {i}"} for i in range(200)]
    digest = build_routing_digest(history, {})
    assert len(digest) < 800
    assert "câu hỏi số 199" in digest
    assert "câu hỏi số 100" not in digest


def test_digest_khong_chua_cau_tra_loi_cua_ai():
    history = [
        {"role": "User", "message": "doanh thu?"},
        {"role": "AI", "message": "Doanh thu tháng 7 đạt 1,24 tỷ đồng " * 50},
    ]
    assert "1,24 tỷ" not in build_routing_digest(history, {})


def test_steering_queue_mo_rong_scope():
    state = {"scoped_modules": ["sales"], ...}
    result = await absorb_steering_node({**state, "_pending": [
        {"mode": "queue", "content": "thêm cả tồn kho nữa"}]})
    assert "sales" in result["scoped_modules"]
    assert "inventory" in result["scoped_modules"]


def test_steering_interrupt_thay_the_scope():
    state = {"scoped_modules": ["sales"], ...}
    result = await absorb_steering_node({**state, "_pending": [
        {"mode": "interrupt", "content": "à nhầm, tôi hỏi về nhân sự"}]})
    assert "sales" not in result["scoped_modules"]
    assert "hr" in result["scoped_modules"]


def test_plan_step_gioi_han_scope():
    """Đang chạy bước plan thì chỉ nạp tool của bước đó + pinned."""
    state = {
        "current_plan_step": {"expectedTools": ["get_low_stock_products"]},
        "scoped_modules": [], "expanded_modules": set(),
        "permitted_tools": ALL_TOOLS,
    }
    scope = build_tool_scope(state)
    assert {t.name for t in scope} == {"get_low_stock_products", "search_knowledge"}


def test_infer_step_tools_loc_ten_bia():
    """Model bịa tên tool khi suy ra expectedTools → phải bị lọc."""
    result = await infer_step_tools("Lấy dữ liệu abcxyz", allowed=[TOOL_A, TOOL_B])
    assert all(n in {"tool_a", "tool_b"} for n in result)


def test_routing_context_khong_chua_so_lieu():
    """Chống tái tạo lỗi C4 (Stage 18.8) qua cửa sau."""
    ctx = extract_entities([{"args": {"product_id": "p1", "total_revenue": 1_240_000_000}}])
    assert "product" in ctx
    assert not any("revenue" in k or "total" in k for k in ctx)


def test_routing_context_het_hieu_luc_sau_30_phut():
    old = {"lastModules": ["product"], "updatedAt": "2026-07-26T06:00:00+07:00"}
    assert not is_follow_up("còn màu đen không?",
                            expire_if_stale(old, now="2026-07-26T09:00:00+07:00"))
```

---

## Definition of Done — Stage 20

### Cơ chế
- [ ] Phễu 4 lớp hoạt động; scope được tính lại ở **mỗi ranh giới bước**, không chỉ đầu run.
- [ ] `ChatSession.RoutingContext` có migration cho **cả** MySQL và PostgreSQL.
- [ ] Thực thể được trích từ **tham số tool đã gọi**, không dùng LLM.
- [ ] `RoutingContext` hết hiệu lực sau 30 phút; đổi chủ đề thì xoá `entities`.
- [ ] `PINNED_TOOLS` chỉ có `search_knowledge`; `request_more_tools` đã **xoá** khỏi Stage 13.3.

### Câu tiếp nối
- [ ] *"Xe SH giá bao nhiêu?"* → *"còn màu đen không?"* → trả lời đúng về SH, **không gọi router**.
- [ ] Câu tiếp nối ở lượt đầu tiên (chưa có ngữ cảnh) → vẫn gọi router bình thường.
- [ ] Fast path đoán sai → tự nạp module đúng trong 1 vòng, không mất câu trả lời.

### Lịch sử dài
- [ ] **Session 200 tin nhắn: digest vẫn < 200 token**, độ chính xác chọn tool không giảm.
- [ ] Digest **không** chứa câu trả lời của AI.
- [ ] `RoutingContext` **không** chứa field số liệu (có test).

### Plan
- [ ] Đang chạy bước plan → nạp **≤ 3 tool + pinned**, không phải 20.
- [ ] User sửa bước plan → `expectedTools` được suy ra lại, tên tool bịa bị lọc.
- [ ] User thêm bước → bước mới có scope riêng.
- [ ] Bước `skipped` → không nạp tool nào.

### Steering
- [ ] Steering `queue` → scope **mở rộng** (union, ≤ 3 module).
- [ ] Steering `interrupt` → scope **thay thế** hoàn toàn.
- [ ] Trần 20 tool vẫn giữ khi union 3 module; cắt theo ưu tiên plan → gốc → bổ sung.

### Đo lường
- [ ] Số tool trung vị nạp/bước **≤ 5**; p95 ≤ 20.
- [ ] Tỉ lệ fast path 25–45%, trong đó sai ≤ 10%.
- [ ] `tool_not_found` ≤ 2%.
- [ ] Eval Stage 13 vẫn pass — **thu hẹp scope không được làm giảm độ chính xác**.
