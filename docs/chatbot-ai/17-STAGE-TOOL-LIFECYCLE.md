# Stage 17 — Vòng đời Tool & Hợp đồng phiên bản

> Nhóm A (12 ca) + E1 + E4 · Ưu tiên: 🔴 Cao · Ước lượng: 3–4 ngày
> Phụ thuộc: **Stage 13, 16** · Nên làm **trước** Stage 15 đợt P1

Gốc rễ của cả nhóm này là một sự thật đơn giản:

> **Tool thay đổi liên tục. Lịch sử hội thoại, plan chờ duyệt, và checkpoint của agent
> thì tồn tại lâu hơn tool.**

> **⚠️ Nợ từ Stage 18 (Consistency) — làm kèm khi xong mục 17.4:**
> - **18.8** (số liệu cũ trong lịch sử hội thoại) — khi sạch hoá lịch sử ở 17.4, tin nhắn cũ hơn
>   15 phút phải được đóng dấu thời gian (`"(Số liệu trong tin nhắn này tính đến {timestamp})"`)
>   và system prompt phải buộc AI tra cứu lại (không đọc số cũ) khi hỏi lại số liệu > 15 phút.
>
> Xem chi tiết: [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md), mục 18.8.

Trong lộ trình 71 tool, mỗi tuần đều có tool được thêm, đổi schema, đổi tên, hoặc gỡ.
Không có cơ chế phiên bản thì mỗi lần đổi là một đợt lỗi âm ỉ mà không ai truy được nguyên nhân.

---

## 17.1. Bản kê các ca cần xử lý

| # | Ca | Xử lý ở mục |
|---|---|---|
| A1 | AI bịa tên tool chưa từng có | 17.3 |
| A3 | Tool đã xoá/đổi tên, lịch sử còn `ToolMessage` cũ → model bắt chước | 17.3, 17.4 |
| A4 | `ToolFlags` chuyển `off` giữa lúc run đang chạy | 17.6 |
| A5 | Permission bị thu hồi giữa phiên, context còn cache | 17.7 |
| A6 | Router chọn nhóm thiếu tool → model bịa tool nhóm khác | 17.3 |
| A8 | Đổi schema tham số, model dùng schema cũ nhớ từ lịch sử | 17.4 |
| A9 | Resume run sau restart, registry đã đổi | 17.8 |
| A10 | Plan chờ duyệt 24h, tool trong plan đã bị gỡ | 17.8 |
| A11 | Sidecar có ToolSpec, endpoint .NET chưa deploy | 17.5 |
| A12 | Endpoint có, `ToolSpec` chưa khai báo | 17.5 |
| E1 | **JWT hết hạn giữa run / trong lúc plan chờ duyệt 24h** | **17.9** |
| E4 | Provider tự cập nhật model → hành vi đổi | 17.10 |
| E2 | Sidecar không restart sau deploy → chạy code cũ | 17.5 |

---

## 17.2. Phiên bản cho ToolSpec và Registry

### Mở rộng `ToolSpec`

```python
@dataclass(frozen=True)
class ToolSpec:
    name: str
    version: int                        # tăng khi ĐỔI schema tham số
    module: str
    required_permissions: tuple[str, ...]
    is_write: bool
    factory: callable
    args_schema: type[BaseModel]
    status: Literal["active", "deprecated", "removed"] = "active"
    replaced_by: str | None = None      # tool thay thế, dùng khi deprecated/removed
    since: str = ""                     # phiên bản dự án tool xuất hiện
```

### Registry manifest — dấu vân tay của toàn bộ tool

```python
def registry_fingerprint() -> str:
    """Hash toàn bộ tool đang hoạt động. Đổi tool → đổi hash."""
    payload = sorted(
        (s.name, s.version, sorted(s.required_permissions))
        for s in TOOL_SPECS if s.status == "active"
    )
    return hashlib.sha256(json.dumps(payload, default=str).encode()).hexdigest()[:16]
```

`fingerprint` này lưu vào `ChatRun` (cột mới `ToolRegistryFingerprint`) và vào plan.
Đây là chìa khoá cho 17.8 và cho Stage 19 (cache plan).

**Migration** (thêm `ChatRun.ToolRegistryFingerprint` và `ChatRun.ModelUsed` ở 17.10) —
tạo cho **cả** MySQL và PostgreSQL:
```powershell
./add-migration.ps1 AddChatRunToolFingerprint
```

**Quy ước tăng `version`:**

| Loại thay đổi | Tăng version? | Cần làm |
|---|---|---|
| Sửa mô tả tool | Không | — |
| Thêm tham số **tuỳ chọn** | Không | — |
| Thêm tham số **bắt buộc** | **Có** | Tool cũ thành `deprecated` |
| Đổi tên / đổi kiểu tham số | **Có** | Như trên |
| Đổi ý nghĩa field trả về | **Có** | Cập nhật `GLOSSARY.md` |
| Gỡ tool | — | `status = "removed"`, đặt `replaced_by` |

---

## 17.3. Xử lý tool không tồn tại (A1, A3, A6)

**Nguyên tắc: không bao giờ để run chết vì tool không tìm thấy. Trả lỗi *có hướng dẫn* cho model.**

```python
async def dispatch_tool(name: str, args: dict, state: AgentState) -> dict:
    spec = TOOL_SPECS.get(name)

    # 1. Tên hoàn toàn không tồn tại → gợi ý tool gần nhất
    if spec is None:
        suggestion = difflib.get_close_matches(name, state["allowed_tool_names"], n=1)
        hint = f" Có phải bạn muốn dùng '{suggestion[0]}'?" if suggestion else ""
        return {
            "error": "tool_not_found",
            "message": (
                f"Không có tool tên '{name}'. Chỉ dùng các tool trong danh sách "
                f"được cung cấp.{hint} Nếu không có tool phù hợp, hãy nói rõ với "
                f"người dùng là bạn chưa hỗ trợ việc này."
            ),
        }

    # 2. Tool đã bị gỡ nhưng có tool thay thế
    if spec.status == "removed":
        if spec.replaced_by:
            return {"error": "tool_removed",
                    "message": f"Tool '{name}' không còn dùng. Hãy dùng '{spec.replaced_by}'."}
        return {"error": "tool_removed",
                "message": f"Tool '{name}' đã bị loại bỏ. Không có tool thay thế."}

    # 3. Tool tồn tại nhưng user không được cấp (A2, A6)
    if name not in state["allowed_tool_names"]:
        return {"error": "tool_not_available",
                "message": (
                    f"Bạn không có quyền dùng '{name}'. Hãy nói với người dùng rằng "
                    f"họ không có quyền truy cập thông tin này. KHÔNG đoán dữ liệu.")}

    return await run_tool(spec, args, state)
```

### Chống lặp lỗi
Cùng một tool không tồn tại bị gọi **2 lần** trong một run → chuyển sang chế độ trả lời không tool:
```python
if state["tool_not_found_count"] >= 2:
    return force_answer_without_tools(state)
```
Nếu không có chặn này, model có thể lặp bịa tên tool cho tới khi hết ngân sách vòng.

### A6 — Router thiếu tool
Khi model gọi tool thuộc module chưa được nạp, **không** trả `tool_not_found` mà cấp thêm module đó:
```python
if spec and spec.module not in state["loaded_modules"]:
    if state["module_expansions"] < 1:          # tối đa 1 lần / run
        state["loaded_modules"].add(spec.module)
        state["module_expansions"] += 1
        return {"info": "module_loaded",
                "message": f"Đã nạp thêm nhóm tool '{spec.module}'. Hãy gọi lại."}
```
Đây là bản tự động hoá của `request_more_tools` ở Stage 13.3.

---

## 17.4. Sạch hoá lịch sử hội thoại (A3, A8)

**Vấn đề cốt lõi:** LLM học rất nhanh từ ví dụ trong ngữ cảnh. Nếu lịch sử chứa
`tool_call: get_stock(product_code="SH150")` mà tham số `product_code` đã đổi thành `product_id`,
model sẽ **tiếp tục dùng schema cũ** ở mọi lượt sau — lỗi tự duy trì.

### Quy tắc: lịch sử nạp lại chỉ giữ **văn bản**, không giữ tool call thô

```python
def sanitize_history(history: list[dict], fingerprint: str) -> list:
    """Chuyển lịch sử từ DB thành message, loại bỏ tool call của phiên bản registry khác."""
    messages = []
    for item in history:
        role = (item.get("role") or "").lower()
        if role == "user":
            messages.append(HumanMessage(content=item["message"]))
        elif role in ("ai", "assistant"):
            # CHỈ giữ nội dung văn bản. Tool call cũ bị bỏ hoàn toàn.
            messages.append(AIMessage(content=item["message"]))
    return messages
```

**Điều này khả thi vì:** `ChatMessage` (Stage 1) chỉ lưu văn bản; tool call nằm ở `ChatRunEvent`
vốn chỉ dùng để tua lại giao diện và bị xoá sau 7 ngày (Stage 8.8). Kiến trúc hiện tại
**đã đúng** — chỉ cần không phá vỡ nó bằng cách nhồi tool call vào `ChatMessage`.

> ⚠️ **Ràng buộc thiết kế cần tôn trọng:** không bao giờ lưu `tool_calls` thô vào `ChatMessage`.
> Nếu cần AI biết đã tra cứu gì ở lượt trước, ghi dưới dạng câu văn:
> *"(Đã tra cứu tồn kho sản phẩm SH 150i lúc 09:15)"* — an toàn với mọi thay đổi schema.

Tool call **trong cùng một run** thì vẫn giữ nguyên (agent cần nó để suy luận tiếp) —
chỉ sạch hoá khi **nạp lại lịch sử từ DB**.

---

## 17.5. Bắt tay tương thích Sidecar ↔ Backend (A11, A12, E2)

Ba nguồn lệch: deploy .NET và Python không đồng thời, sidecar không restart, hoặc dev quên
khai báo một bên.

### Endpoint kiểm kê ở .NET
```
GET /internal/chat/tools/manifest
→ { "tools": ["get_stock_on_hand", "get_sales_summary", ...], "buildId": "2026.07.26.1" }
```
Sinh bằng reflection từ các action có `[ChatTool]` attribute — **không** duy trì danh sách tay
(danh sách tay sẽ lệch).

### Sidecar tự kiểm lúc khởi động

```python
async def verify_tool_contract() -> None:
    """So khớp ToolSpec với endpoint thật. Chạy trong FastAPI lifespan."""
    backend_tools = set((await backend.get_tool_manifest())["tools"])
    local_tools = {s.name for s in TOOL_SPECS if s.status == "active"}

    missing_backend = local_tools - backend_tools   # A11: sidecar có, backend chưa
    missing_spec    = backend_tools - local_tools   # A12: backend có, sidecar chưa khai

    if missing_backend:
        logger.error("Tool chưa có endpoint ở backend: %s", sorted(missing_backend))
        for name in missing_backend:
            TOOL_SPECS[name].status = "removed"     # tự vô hiệu, không để 404 hàng loạt

    if missing_spec:
        logger.warning("Endpoint chưa được khai báo ToolSpec: %s", sorted(missing_spec))
```

**Hành vi khi lệch:** tool thiếu endpoint bị **tự động vô hiệu**, không để AI gọi rồi nhận 404.
Chỉ log cảnh báo, không làm sidecar chết — deploy lệch vài phút là bình thường.

### E2 — Sidecar chạy code cũ

`AiSidecarManager` truyền `BUILD_ID` (từ assembly version) xuống env. Sidecar so với build id
của chính nó:
```csharp
startInfo.EnvironmentVariables["EXPECTED_BUILD_ID"] = buildId;
```
Lệch → log `ERROR` và trả cờ `stale` ở `GET /health`, để health check của Stage 6.7 bắt được.

**Bổ sung vào quy trình deploy** (`.github/workflows/deploy.yml`): sau khi restart backend, gọi
`/health` và **fail deploy** nếu `stale = true`.

---

## 17.6. `ToolFlags` đổi giữa lúc run đang chạy (A4)

**Nguyên tắc: run đã bắt đầu thì dùng ảnh chụp cấu hình lúc bắt đầu.** Đổi cờ chỉ áp cho run mới.

```python
# Chụp lúc khởi tạo run, giữ trong AgentState suốt run
state["tool_flags_snapshot"] = await backend.get_tool_flags()
state["registry_fingerprint"] = registry_fingerprint()
```

**Ngoại lệ — tắt khẩn cấp:** khi phát hiện tool trả số sai trong production (Stage 16.8),
cần tắt **ngay**, kể cả run đang chạy. Dùng cờ riêng:

```jsonc
"ToolFlags": {
    "get_pnl_report": "off",
    "_killSwitch": ["get_payroll_summary"]    // áp ngay, kể cả run đang chạy
}
```
Tool trong `_killSwitch` bị kiểm tra lại **trước mỗi lần gọi**, không dùng snapshot.
Run đang chạy gặp tool bị kill → trả `tool_not_available` và AI trả lời phần đã có.

---

## 17.7. Permission bị thu hồi giữa phiên (A5)

Context cache 5 phút (Stage 14.3a) tạo cửa sổ 5 phút mà user đã bị thu hồi quyền vẫn dùng được tool.

### Ba lớp
1. **Invalidate chủ động:** backend gọi `POST /internal/cache/invalidate` khi cập nhật
   role/permission của user. Bổ sung vào các handler của `Features/Permissions/` và
   `Features/UserManager/`.
2. **Luôn lấy mới ở đầu mỗi run:** cache chỉ dùng **trong** một run, không dùng lại giữa các run.
   Một run kéo dài tối đa 5 phút nên cửa sổ rủi ro bằng đúng thời lượng một run — chấp nhận được.
   ```python
   # Cache key gồm run_id → tự động hết hiệu lực khi run kết thúc
   cache_key = f"{session_id}:{run_id}"
   ```
3. **Backend vẫn là hàng rào thật:** mọi endpoint tool check permission độc lập (Stage 13.2).
   Dù registry còn tool, gọi vào vẫn nhận 403.

> Lớp 3 khiến A5 không phải lỗ hổng bảo mật, chỉ là trải nghiệm xấu (AI tưởng gọi được rồi nhận 403).
> Lớp 1 và 2 để trải nghiệm đúng.

---

## 17.8. Resume và Plan hết hiệu lực (A9, A10)

### Revalidate trước khi tiếp tục

Mọi điểm "tiếp tục việc cũ" đều phải kiểm tra lại registry:

```python
async def revalidate_before_resume(run: ChatRun) -> RevalidationResult:
    current = registry_fingerprint()
    if run.tool_registry_fingerprint == current:
        return RevalidationResult.ok()

    # Registry đã đổi — kiểm tra từng tool mà run/plan cần
    needed = extract_tools_from_plan(run.plan)
    unavailable = [t for t in needed
                   if t not in TOOL_SPECS or TOOL_SPECS[t].status == "removed"]

    if not unavailable:
        return RevalidationResult.ok(note="registry_changed_but_compatible")

    return RevalidationResult.degraded(unavailable)
```

### Hành vi theo mức độ

| Tình huống | Hành vi |
|---|---|
| Fingerprint khớp | Tiếp tục bình thường |
| Fingerprint đổi, tool cần vẫn đủ | Tiếp tục, ghi event `registry_changed` |
| **Có tool trong plan đã bị gỡ** | Chuyển plan về `Drafting`, đánh dấu bước liên quan `status = "invalid"`, phát event `plan_invalidated`, **yêu cầu duyệt lại** |
| Toàn bộ tool cần đã bị gỡ | Run → `Failed` với `ErrorCode = plan_obsolete`, FE hiện nút "Hỏi lại" |

### FE hiển thị
Plan card (Stage 10.7) khi nhận `plan_invalidated`:
> ⚠️ Hệ thống đã cập nhật, một số bước trong kế hoạch không còn khả dụng.
> Bước 3 cần được thay thế. [Xem lại kế hoạch]

Bước `invalid` hiện gạch ngang + badge đỏ, nút Duyệt bị khoá tới khi user xử lý.

---

## 17.9. JWT hết hạn giữa run (E1) — quyết định thiết kế

Đây là **lỗi thiết kế**, không phải lỗi triển khai. Cần chốt trước khi làm Stage 10.

### Vấn đề
- `StreamManagerChatMessageCommandHandler` truyền JWT của user xuống sidecar; sidecar dùng nó gọi
  tool endpoint.
- Run nền (Stage 8) sống tối đa 5 phút — vẫn có thể vượt hạn nếu token gần hết.
- **Plan chờ duyệt tới 24 giờ** (Stage 10.5) — token chắc chắn hết hạn. Duyệt xong thì mọi tool 401.

### Ba phương án

| | Phương án | Ưu | Nhược |
|---|---|---|---|
| **A** | **Run token riêng**: backend cấp token ngắn hạn, scope hẹp (chỉ `/internal/chat/tools/*`), gắn với `runId`, tự gia hạn bởi executor | Không phụ thuộc token user; kiểm soát scope chặt; audit rõ | Phải viết cơ chế cấp/thu hồi token |
| **B** | Refresh token: executor giữ refresh token của user, tự làm mới | Tái dùng hạ tầng auth có sẵn | Lưu refresh token phía server = rủi ro; user logout thì run chết |
| **C** | Rút timeout plan xuống trong hạn token (ví dụ 30 phút) | Không cần code gì | Mất tính năng "duyệt sau" — trái mục tiêu Stage 10 |

### Khuyến nghị: **Phương án A**

```csharp
// Application/Interfaces/Services/IChatRunTokenService.cs
public interface IChatRunTokenService
{
    /// <summary>Cấp token chỉ dùng được cho tool endpoint của đúng run này.</summary>
    string IssueRunToken(Guid runId, Guid userId, IReadOnlyList<string> permissions,
                         TimeSpan lifetime);

    /// <summary>Xác thực và trả về danh tính gắn với run.</summary>
    RunTokenClaims? Validate(string token, Guid expectedRunId);
}
```

Đặc tính bắt buộc của run token:
- **Scope hẹp:** chỉ hợp lệ với `/internal/chat/tools/*`, không dùng được cho API nghiệp vụ khác.
- **Gắn `runId`:** token của run A không dùng được cho run B.
- **Ảnh chụp permission tại thời điểm cấp**, nhưng **vẫn** revalidate với DB ở mỗi lần gọi tool
  (để 17.7 lớp 3 còn hiệu lực — token không phải là bằng chứng quyền vĩnh viễn).
- **Thời hạn ngắn** (15 phút), executor tự gia hạn khi run còn sống.
- **Thu hồi khi:** run kết thúc, user logout, hoặc permission thay đổi.

Với plan chờ duyệt 24h: token cũ hết hạn là chuyện bình thường — lúc user bấm **Duyệt**,
request đó mang JWT còn hiệu lực của user, backend **cấp token run mới** rồi mới thực thi.
Đây là điểm mấu chốt khiến phương án A giải quyết được cả A10 và E1 cùng lúc.

> **Bắt buộc:** revalidate permission tại thời điểm duyệt, không dùng permission đã chụp 24h trước.
> User có thể đã bị đổi vai trò trong thời gian chờ.

---

## 17.10. Phiên bản model (E4)

Provider cập nhật model sau lưng → eval hôm qua xanh, hôm nay đỏ, không ai đổi code.

| Việc | Chi tiết |
|---|---|
| **Ghim phiên bản cụ thể** | Nếu provider hỗ trợ tên có hậu tố phiên bản, dùng bản ghim thay vì alias trỏ "mới nhất" |
| **Ghi model thật vào mỗi run** | Cột mới `ChatRun.ModelUsed` — lưu tên model từ response metadata, **không** từ config |
| **Eval hồi quy định kỳ** | Chạy `guardrail_cases.yaml` + `rag_cases.yaml` hằng tuần theo lịch, không chỉ khi có PR |
| **Cảnh báo lệch** | `ModelUsed` khác `AISetup:Model` → log ERROR |
| **Ghi nhật ký thay đổi** | File `docs/chatbot-ai/MODEL-CHANGELOG.md`: ngày, model, kết quả eval trước/sau |

> Giữ nguyên `gemini-3.5-flash` theo quyết định ở Stage 1.2 — mục này chỉ là cơ chế phát hiện
> khi provider thay đổi hành vi của model đó.

---

## 17.11. Chính sách khai tử tool

Để đổi tool không thành sự cố:

```
Thêm tool mới  → status = active, ToolFlags = shadow (Stage 16.8)
Đổi schema     → tool mới version+1 (active) + tool cũ deprecated, chạy song song ≥ 2 tuần
Gỡ tool        → deprecated ≥ 2 tuần → removed (giữ ToolSpec với replaced_by ≥ 3 tháng)
Xoá ToolSpec   → chỉ sau 3 tháng ở trạng thái removed
```

Tool `deprecated` **vẫn hoạt động** nhưng không đưa vào registry của user mới; mô tả thêm tiền tố
`[SẮP NGỪNG]` để model ưu tiên tool mới.

**Giữ `ToolSpec` với `status = removed`** là điểm quan trọng nhất của chính sách này — nhờ nó,
17.3 mới trả được thông báo *"tool này đã bị gỡ, hãy dùng X"* thay vì *"không tìm thấy tool"*
vô nghĩa với model.

---

## Definition of Done — Stage 17

- [ ] `ToolSpec` có `version`, `status`, `replaced_by`; quy ước tăng version được ghi trong `RULES.md`.
- [ ] `registry_fingerprint()` hoạt động, lưu vào `ChatRun.ToolRegistryFingerprint`.
- [ ] Gọi tool bịa → nhận thông báo có gợi ý tool gần nhất, run **không** chết.
- [ ] Gọi tool bịa 2 lần → chuyển sang trả lời không dùng tool.
- [ ] Gọi tool thuộc module chưa nạp → tự nạp module (tối đa 1 lần/run).
- [ ] Lịch sử nạp lại từ DB **không** chứa tool call thô; đổi schema tham số không gây lỗi lặp ở lượt sau.
- [ ] `GET /internal/chat/tools/manifest` sinh tự động bằng reflection.
- [ ] Sidecar tự kiểm hợp đồng lúc khởi động; tool thiếu endpoint bị tự vô hiệu + log ERROR.
- [ ] Deploy lệch build id → `/health` báo `stale`, workflow deploy fail.
- [ ] Đổi `ToolFlags` giữa run → run đang chạy không bị ảnh hưởng; `_killSwitch` áp ngay.
- [ ] Thu hồi permission → cache bị invalidate; run mới không còn tool đó.
- [ ] Resume run sau khi gỡ tool → plan chuyển `Drafting`, bước liên quan `invalid`, FE hiện cảnh báo.
- [ ] **Duyệt plan sau 24h vẫn thực thi được** (run token mới được cấp lúc duyệt).
- [ ] Permission được revalidate tại thời điểm duyệt plan, không dùng bản chụp cũ.
- [ ] Run token không dùng được cho API ngoài `/internal/chat/tools/*` (có test).
- [ ] Run token của run A không dùng được cho run B (có test).
- [ ] `ChatRun.ModelUsed` ghi model thật từ response metadata.
- [ ] Eval hồi quy chạy theo lịch hằng tuần.

### Test

`AISidecar/tests/test_tool_lifecycle.py`:
- [ ] `registry_fingerprint()` **tất định** — gọi 2 lần cùng registry cho cùng hash.
- [ ] Đổi `version` của 1 tool → fingerprint đổi; sửa mô tả → **không** đổi.
- [ ] `dispatch_tool` tên bịa → `tool_not_found` + gợi ý tool gần nhất, **không** ném exception.
- [ ] Gọi tool bịa lần 2 → `force_answer_without_tools`.
- [ ] `status="removed"` có `replaced_by` → thông báo nêu tên tool thay thế.
- [ ] `sanitize_history` — lịch sử chứa tool call cũ → output **không** có `tool_calls` nào.
- [ ] `verify_tool_contract` — tool thiếu endpoint → tự set `removed` + log ERROR,
      sidecar **vẫn khởi động được**.
- [ ] `_killSwitch` áp ngay cả khi snapshot cờ đã chụp từ đầu run.

`UnitTests/ChatRunToken.cs`:
- [ ] Run token **không** dùng được cho API ngoài `/internal/chat/tools/*`.
- [ ] Run token của run A **không** dùng được cho run B.
- [ ] Token hết hạn giữa run → executor gia hạn, tool call kế tiếp thành công.
- [ ] Duyệt plan sau 24h → cấp token mới **và** revalidate permission tại thời điểm duyệt.
