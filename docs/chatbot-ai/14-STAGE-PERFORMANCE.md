# Stage 14 — Tối ưu tốc độ & số lần suy nghĩ

> Yêu cầu #5 (phần tốc độ) · Ưu tiên: 🟠 Trung bình · Ước lượng: 2–3 ngày · Phụ thuộc: **Stage 3, 13**
> Mục tiêu: giảm thời gian chờ và giảm số vòng lặp của agent — vừa nhanh hơn vừa rẻ hơn.

> **⚠️ Nợ từ Stage 18 (Consistency) — làm kèm khi xong Stage này:**
> - **14.5 (Cache nhiều tầng):** thay TTL 60s bằng `RunSnapshot` (đã có ở
>   `AISidecar/app/services/run_snapshot.py`, Stage 18.2) cho các tool đọc trong cùng một run —
>   nhất quán hơn mà vẫn tiết kiệm y như cũ. Cache theo run tự động thoả điều kiện "key theo user"
>   vì run đã gắn 1 user.
> - **14.6 (Tóm tắt lịch sử hội thoại):** áp 3 quy tắc của Stage 18.9 — không tóm tắt số liệu,
>   giữ nguyên văn 15 tin gần nhất, lưu `SummarizedUpToMessageId` vào `ChatSession` (cần migration
>   MySQL + PostgreSQL).
>
> Xem chi tiết: [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md), mục 18.2, 18.9.

---

## 14.1. Đo trước, tối ưu sau

Không tối ưu khi chưa có số. Thêm đo đạc vào mọi run (tận dụng `ChatRunEvent` của Stage 8):

| Chỉ số | Ý nghĩa | Mục tiêu |
|---|---|---|
| **TTFT** (time to first token) | Thời gian tới ký tự đầu tiên user nhìn thấy | < 1.5s |
| **TTFA** (time to full answer) | Tổng thời gian tới khi trả lời xong | < 8s (không tool), < 15s (có tool) |
| **Số vòng agent** | Số lần gọi LLM trong 1 run | ≤ 3 (trung vị) |
| **Số tool call** | | ≤ 2 (trung vị) |
| **Token in / out** | Chi phí | Theo dõi xu hướng |

Ghi các chỉ số này vào log JSON của sidecar (Stage 7.7) và metadata LangSmith (Stage 6.6).

> **TTFT là chỉ số quan trọng nhất về cảm nhận.** Người dùng chịu được câu trả lời mất 10 giây
> nếu chữ bắt đầu chạy sau 1 giây. Ngược lại, im lặng 4 giây rồi trả lời tức thì lại thấy chậm.

---

## 14.2. Giảm số vòng suy nghĩ

Đây là đòn bẩy lớn nhất — mỗi vòng agent là **một lần round-trip LLM đầy đủ** (~1–3s).

### a) Fast path — bỏ qua agent hoàn toàn

Nhiều câu hỏi không cần tool. Router ở [13-STAGE-GUARDRAILS.md](13-STAGE-GUARDRAILS.md) mục 13.3
đã phân loại `none` → trả lời thẳng bằng 1 lần gọi LLM, **0 vòng agent**.

Mở rộng fast path cho:
- Chào hỏi, cảm ơn, tán gẫu
- Hỏi lại về nội dung đã có trong lịch sử hội thoại ("vừa nãy bạn nói gì?")
- Hỏi về khả năng của chatbot ("bạn làm được gì?")

Ước tính 30–40% lượt chat rơi vào fast path.

### b) Tool gộp — thiết kế để 1 lần gọi là đủ

Thay vì bắt agent gọi 3 tool nối tiếp, gộp thành 1 tool trả đủ dữ liệu:

| Thay vì | Dùng |
|---|---|
| `search_products` → `get_product_stock` → `get_product_price` | `search_products` trả luôn tồn kho + giá |
| `get_sales_summary(tháng này)` → `get_sales_summary(tháng trước)` | `get_sales_summary` nhận `compare_with_previous: bool` |
| `semantic_product_search` → `get_products_by_ids` | Gộp trong 1 tool (đã nêu ở Stage 12.5) |

**Nguyên tắc thiết kế tool:** nghĩ theo *câu hỏi của người dùng*, không theo *endpoint của API*.
Người dùng hỏi "doanh thu tháng này so với tháng trước" là **một** câu hỏi → nên là **một** tool.

### c) Song song hoá tool call

Gemini hỗ trợ trả nhiều tool call trong một lượt. LangGraph chạy chúng song song nếu node
tool được viết đúng:

```python
async def call_tools_node(state: AgentState) -> dict:
    calls = state["messages"][-1].tool_calls
    results = await asyncio.gather(*[
        run_tool(c["name"], c["args"]) for c in calls
    ], return_exceptions=True)
    return {"messages": [to_tool_message(c, r) for c, r in zip(calls, results)]}
```

Thêm vào system prompt:
```markdown
Nếu cần nhiều thông tin độc lập nhau, hãy gọi TẤT CẢ các tool cần thiết
trong CÙNG một lượt thay vì gọi lần lượt.
```

3 tool tuần tự (4.5s) → 3 tool song song (1.8s).

### d) Trần cứng số vòng

Đã đặt ở Stage 13.6: 6 vòng thường, 12 vòng plan mode. Theo dõi phân bố thực tế —
nếu trung vị > 3, mô tả tool đang kém hoặc tool chưa gộp đủ.

---

## 14.3. Giảm TTFT

### a) Không chặn stream bởi việc lấy context

Hiện `manager_chat_controller.py` gọi `/internal/chat/context` **rồi mới** bắt đầu LLM
→ cộng thẳng 100–300ms vào TTFT.

**Cách sửa:** cache context theo `session_id`, TTL 5 phút. User/roles/permissions gần như không đổi
trong một phiên chat.

```python
@dataclass
class CachedContext:
    data: dict
    expires_at: float

_context_cache: dict[str, CachedContext] = {}

async def get_context_cached(session_id: str, ...) -> dict:
    hit = _context_cache.get(session_id)
    if hit and hit.expires_at > time.monotonic():
        return hit.data
    data = await backend.get_context(session_id, ...)
    _context_cache[session_id] = CachedContext(data, time.monotonic() + 300)
    return data
```

> **Lưu ý bảo mật:** cache **phải** key theo `session_id` (đã gắn với 1 user), không key theo
> câu hỏi. Và phải **xoá cache khi permission của user thay đổi** — backend gọi
> `POST /internal/cache/invalidate` khi cập nhật role/permission. Không có invalidation thì
> user bị thu hồi quyền vẫn dùng được tool trong tối đa 5 phút.

> ⚠️ **Cách xử lý dứt điểm:** key cache theo `session_id:run_id` thay vì chỉ `session_id`.
> Cache tự hết hiệu lực khi run kết thúc → cửa sổ rủi ro bằng đúng thời lượng một run (≤ 5 phút)
> và không cần TTL. Kèm invalidate chủ động và revalidate ở backend.
> Ba lớp đầy đủ ở [17-STAGE-TOOL-LIFECYCLE.md](17-STAGE-TOOL-LIFECYCLE.md) mục 17.7.

Lịch sử hội thoại thì **không cache** (đổi mỗi lượt) — tách thành request riêng, hoặc lấy
kèm nhưng phần user/permissions dùng cache.

### b) Kết nối HTTP dùng lại

Hiện mỗi lần gọi tạo `httpx.AsyncClient` mới → TCP + TLS handshake mỗi lần.

```python
# app/main.py
@asynccontextmanager
async def lifespan(app: FastAPI):
    app.state.http = httpx.AsyncClient(
        timeout=15.0,
        limits=httpx.Limits(max_keepalive_connections=20, max_connections=50),
    )
    yield
    await app.state.http.aclose()
```
Tiết kiệm 20–80ms mỗi lời gọi backend.

### c) Phát tín hiệu sớm cho người dùng

Ngay khi run bắt đầu, phát event `thinking` đầu tiên (Stage 11) trước khi LLM trả token:
> 💭 Đang tìm hiểu yêu cầu của bạn...

Đây là "TTFT cảm nhận" — người dùng thấy phản hồi trong < 300ms dù token thật chưa tới.

### d) Rút ngắn system prompt

System prompt được gửi lại **mỗi lượt**. Prompt 2000 token × 20 lượt = 40.000 token thừa.

- Đưa phần ít đổi vào prompt caching (mục 14.4).
- Danh sách permission: thay vì liệt kê đủ 185 chuỗi dài, gom theo module:
  `"Quyền: Kho (đầy đủ), Bán hàng (chỉ xem), Nhân sự (không có)"`.
  Ngắn hơn ~10 lần và model hiểu tốt hơn.

---

## 14.4. Chọn model theo việc

Dùng **một** khoá `AISetup:Model` cho mọi tác vụ. Tối ưu bằng **tham số gọi**, không tách model:

| Việc | Cấu hình gọi |
|---|---|
| Router phân nhóm (13.3) | `temperature=0`, `max_output_tokens=16` |
| Phân loại steering (9.4) | `temperature=0`, `max_output_tokens=8` |
| Sinh tiêu đề (Stage 4.1) | `temperature=0.3`, `max_output_tokens=32` |
| Agent chính | `temperature=0.7` |
| Tổng hợp cuối | `temperature=0.5` |

**`max_output_tokens` thấp cho tác vụ phân loại là tối ưu bị bỏ quên nhiều nhất** — model
dừng sớm thay vì sinh giải thích dài dòng không ai đọc.

> Nếu sau này muốn tách model riêng (ví dụ model rẻ cho routing), thêm khoá lúc đó — không khai trước.

### Prompt caching
Nếu provider hỗ trợ, đánh dấu phần tĩnh của system prompt (hướng dẫn chung, mô tả tool) là
cacheable. Phần động (tên user, permission, thời gian) đặt **ở cuối**, sau phần tĩnh.

> Thứ tự này quan trọng: cache chỉ khớp theo tiền tố. Đặt tên user ở đầu prompt là hỏng cache.

---

## 14.5. Cache nhiều tầng

| Cache | Key | TTL | Lợi ích |
|---|---|---|---|
| Context user | `session_id` | 5 phút | −150ms/lượt |
| Embedding câu hỏi | `sha256(query)` | 1 giờ | −100ms + 1 API call |
| Kết quả tool chỉ-đọc | `tool + args + user_id` | 60 giây | −200ms, tránh gọi trùng trong 1 run |
| Prompt tĩnh | provider tự lo | — | −token |
| Danh sách tool đã build | `frozenset(permissions)` | 10 phút | −CPU |

**Cache kết quả tool phải key theo `user_id`** — nếu không, user A thấy dữ liệu user B.
Đây là lỗi bảo mật nghiêm trọng và rất dễ mắc. Viết test riêng cho nó.

Dữ liệu biến động nhanh (tồn kho, đơn hàng đang xử lý) → TTL ngắn (30–60s) hoặc không cache.
Dữ liệu tĩnh (danh mục, thương hiệu, chính sách) → TTL dài (10 phút).

> ⚠️ **TTL khác nhau giữa các tool tạo ra câu trả lời không nhất quán nội tại:** AI nói
> *"còn 12 xe, giá 98 triệu"* với hai con số đọc ở hai thời điểm khác nhau.
> **Cách sửa: thay TTL bằng cache theo phạm vi run (`RunSnapshot`)** — tiết kiệm y như cũ nhưng
> mọi tool trong một run đọc cùng một ảnh chụp dữ liệu. Đồng thời tự thoả mãn yêu cầu key theo
> user (run đã gắn với đúng một user). Xem
> [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.2.

---

## 14.6. Tối ưu lịch sử hội thoại

Lịch sử dài làm chậm mọi lượt (nhiều token vào = chậm hơn + đắt hơn).

| Số tin nhắn | Chiến lược |
|---|---|
| ≤ 20 | Gửi nguyên |
| 21–40 | Sliding window 20 tin gần nhất (Stage 2.4) |
| > 40 | Tóm tắt tin cũ thành 1 đoạn + 15 tin gần nhất |

Tóm tắt chạy **nền, không chặn**: sau khi run kết thúc, nếu session > 40 tin thì kích một job
tóm tắt lưu vào `ChatSession.Summary`. Lượt sau dùng luôn.

> ⚠️ **Tóm tắt làm mất chi tiết → lượt sau trả lời lệch.** Quy tắc bắt buộc:
> **không tóm tắt số liệu**, chỉ tóm tắt chủ đề và quyết định; giữ nguyên văn 15 tin gần nhất;
> tóm tắt phải ghi rõ *"chi tiết số liệu đã lược bỏ, hãy tra cứu lại nếu cần"*.
> Xem [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.9.

---

## 14.7. Song song hoá phía backend

Trong `ChatRunExecutor` (Stage 8), các việc sau chạy song song thay vì tuần tự:

```csharp
var contextTask = GetContextAsync(...);
var historyTask = GetHistoryAsync(...);
var permsTask   = GetPermissionsAsync(...);
await Task.WhenAll(contextTask, historyTask, permsTask);
```

Và ghi `ChatRunEvent` không được chặn stream. **Không dùng batching** (Stage 8.4 đã bỏ —
quyết định 2026-07-29: batching làm chữ hiện "nhảy cục", ưu tiên tự nhiên hơn số lần ghi DB).
Nếu tần suất ghi DB thật sự thành nút thắt, tối ưu đúng hướng là giảm chi phí mỗi lần ghi
(batch insert phía driver, async fire-and-forget có kiểm soát...), **không phải** gộp nhiều
`text_delta` thành một trước khi hiển thị cho user.

---

## 14.8. Bảng tổng hợp tác động

Ước tính cho câu hỏi điển hình có tool:

| Tối ưu | Tiết kiệm | Công sức |
|---|---|---|
| Fast path cho câu không cần tool | −3 đến −6s (30–40% lượt) | Thấp |
| Gộp tool để giảm 1 vòng agent | −1.5 đến −3s | Trung bình |
| Song song hoá tool call | −1 đến −3s | Thấp |
| Cache context | −150ms | Thấp |
| HTTP client dùng lại | −50ms/lời gọi | Rất thấp |
| Rút gọn system prompt | −200ms + token | Thấp |
| `max_output_tokens` cho router | −300ms | Rất thấp |
| Cache kết quả tool | −200ms | Trung bình |
| Tóm tắt lịch sử dài | −500ms với session dài | Trung bình |

**Làm theo thứ tự công sức thấp → cao.** Bốn mục đầu bảng đã giải quyết phần lớn vấn đề.

---

## 14.9. Cạm bẫy

| Cạm bẫy | Hậu quả |
|---|---|
| Cache kết quả tool không key theo user | **Rò rỉ dữ liệu giữa các user** |
| Cache context không invalidate khi đổi quyền | User bị thu hồi quyền vẫn dùng được tool |
| Giảm số vòng quá tay | AI trả lời thiếu thay vì tra cứu tiếp |
| Gộp tool quá nhiều | Tool trả dữ liệu thừa → nhiều token hơn cả việc gọi 2 lần |
| Tối ưu trước khi đo | Tốn công vào chỗ không phải nút cổ chai |
| Đặt phần động ở đầu prompt | Hỏng prompt cache |

---

## Definition of Done — Stage 14

- [ ] TTFT, TTFA, số vòng, số tool call, token được ghi log cho **mọi** run.
- [ ] Có dashboard/query xem được p50 và p95 của các chỉ số trên.
- [ ] Câu chào hỏi → 0 tool call, TTFA < 2s.
- [ ] Câu hỏi có tool → trung vị số vòng agent ≤ 3.
- [ ] TTFT < 1.5s ở p50.
- [ ] Nhiều tool độc lập được gọi **song song**, không tuần tự.
- [ ] Context được cache và **có cơ chế invalidate khi đổi permission** (có test).
- [ ] Cache kết quả tool key theo `user_id` (có test chứng minh user A không thấy dữ liệu user B).
- [ ] `httpx.AsyncClient` dùng chung theo vòng đời app.
- [ ] Router dùng `Model` với `max_output_tokens` thấp.
- [ ] Bộ eval ở Stage 13 vẫn pass sau tối ưu — **tốc độ không được đổi bằng độ chính xác**.
