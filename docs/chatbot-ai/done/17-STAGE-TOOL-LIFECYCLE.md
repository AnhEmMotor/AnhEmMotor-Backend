# Stage 17 — Vòng đời Tool & Hợp đồng phiên bản

> Nhóm A (12 ca) + E1 + E4 · Ưu tiên: 🔴 Cao · Ước lượng: 3–4 ngày
> Phụ thuộc thật: **Stage 13, 20** (đã xong — xem `done/`). Dòng "Phụ thuộc: Stage 13, 16" cũ mâu
> thuẫn với thứ tự thực hiện ở [00-OVERVIEW.md](../00-OVERVIEW.md) mục 4 (17 làm **trước** 16) — đã sửa.

> **Trạng thái (2026-07-29): triển khai phần lõi xong, 6 mục hoãn sang Stage 10.**
> Quy mô thực tế hiện tại là 6 tool thật (chưa tới Stage 15 — 71 tool) và **chưa có Plan Mode**
> (Stage 10 chưa xây). Các mục 17.8 và phần "run token riêng cho 24h chờ duyệt" của 17.9 giả định
> có Plan Mode nên **chưa code** — xây khi làm Stage 10, xem ghi chú ở
> [10-STAGE-PLAN-MODE.md](../10-STAGE-PLAN-MODE.md). Chi tiết từng mục ở Definition of Done cuối file.
>
> **Bug thật gặp khi test thủ công, đã vá tạm — sửa tận gốc ở Stage 11:** model viết narration bịa
> (tên sản phẩm/thương hiệu không có thật) cùng lượt với `tool_calls`, thoáng hiện ra khi stream
> trước khi kịp bị xoá. Đã vá tạm ở `call_model_node` (xoá text đi kèm `tool_calls`). Xem ghi chú
> đầy đủ + hướng sửa tận gốc bằng thẻ `<suy_nghi>` ở
> [11-STAGE-REASONING-TRANSPARENCY.md](../11-STAGE-REASONING-TRANSPARENCY.md).

Gốc rễ của cả nhóm này là một sự thật đơn giản:

> **Tool thay đổi liên tục. Lịch sử hội thoại, plan chờ duyệt, và checkpoint của agent
> thì tồn tại lâu hơn tool.**

> **⚠️ Nợ từ Stage 18 (Consistency) — làm kèm khi xong mục 17.4:**
> - **18.8** (số liệu cũ trong lịch sử hội thoại) — khi sạch hoá lịch sử ở 17.4, tin nhắn cũ hơn
>   15 phút phải được đóng dấu thời gian (`"(Số liệu trong tin nhắn này tính đến {timestamp})"`)
>   và system prompt phải buộc AI tra cứu lại (không đọc số cũ) khi hỏi lại số liệu > 15 phút.
>
> Xem chi tiết: [18-STAGE-CONSISTENCY.md](../18-STAGE-CONSISTENCY.md), mục 18.8.

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
state["tool_flags_snapshot"] = get_settings().tool_flags
```

> **Đã bỏ ý tưởng "`_killSwitch` check-live, áp ngay cả run đang chạy" ở bản kế hoạch gốc.**
> `Settings` (Python, `app/config.py`) bị `@lru_cache` theo tiến trình — đổi env var chỉ có tác
> dụng sau khi **restart** sidecar, bất kể check "mỗi dispatch" hay "chụp đầu run": trong cùng một
> tiến trình, giá trị đã cache không đổi. Tách riêng thành 1 endpoint admin runtime
> (`POST /tool-kill-switch`) để "tắt không cần restart" hoá ra giải quyết vấn đề không tồn tại,
> mà lại mở thêm 1 bề mặt tấn công (tắt tool từ xa qua HTTP). Quyết định 2026-07-29: bỏ endpoint,
> chỉ dùng **một** cơ chế — `TOOL_FLAGS` env var, đổi giá trị + restart sidecar. Muốn gỡ tool hẳn
> (không chỉ tạm tắt), sửa `status: "removed"` trong catalog — cũng cần restart.

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

- [x] `ToolSpec` có `version`, `status`, `replaced_by`, `since`; quy ước tăng version ghi ở
      [RULES.md](../RULES.md). (`AISidecar/app/tools/registry.py`, `SharedConfig/chat-tools-catalog.json`)
- [x] `registry_fingerprint()` hoạt động, lưu vào `ChatRun.ToolRegistryFingerprint` cuối mỗi run
      (event `run_meta` từ sidecar → `ChatRunWriter.SetRunMetaAsync`).
- [x] Gọi tool bịa → nhận thông báo có gợi ý tool gần nhất, run **không** chết.
- [x] Gọi tool bịa 2 lần → chuyển sang trả lời không dùng tool (`tools_disabled`, không bind tool
      nào ở lượt kế tiếp).
- [x] Gọi tool thuộc module chưa nạp → tự nạp module (tối đa 1 lần/run) — dispatch trực tiếp từ
      `call_tools_node`, không chỉ từ steering như bản đầu của Stage 20.
- [x] Lịch sử nạp lại từ DB **không** chứa tool call thô (đã đúng sẵn từ Stage 1, khoá lại bằng test
      `test_sanitize_history_khong_giu_tool_call`).
- [x] `GET /internal/chat/tools/manifest` — **không dùng attribute `[ChatTool]` + reflection thủ công
      như bản kế hoạch gốc.** Dùng lại `IChatToolCatalogProvider` (nguồn thật có sẵn, đọc
      `chat-tools-catalog.json` — file này vốn đã dùng chung cho cả sidecar lẫn backend) + trả `Status`
      theo catalog. Lý do đổi: catalog vốn đã là single-source-of-truth giữa 2 phía, tự chế reflection
      thêm là trùng lặp không cần thiết ở quy mô 6 tool hiện tại.
- [x] Sidecar tự kiểm hợp đồng lúc khởi động (`verify_tool_contract`, gọi trong FastAPI lifespan);
      tool thiếu endpoint bị tự vô hiệu (`_locally_disabled`) + log ERROR; sidecar vẫn khởi động được.
- [x] `/health` báo `stale: true` khi build id lệch (`EXPECTED_BUILD_ID` do `AiSidecarManager` truyền,
      so với build id backend trả về live qua manifest, cache 60s để tránh gọi backend mỗi lần ping).
      **Chưa làm:** fail CI/CD workflow khi `stale=true` — là thay đổi ở `.github/workflows/deploy.yml`,
      ngoài phạm vi code lần này, cần làm riêng.
- [x] `ToolFlags` (`app/config.py::tool_flags`, dict `{"tool_name": "off"}`) — tool bị `off` không
      được cấp cho run mới. **Đã bỏ khối "kill switch qua endpoint runtime" từng làm ở đây** — vì
      `Settings` bị `@lru_cache` nên đổi env var vốn dĩ **chỉ có tác dụng sau khi restart** dù có
      check "live" mỗi dispatch hay không (không có gì để "live" cả — cùng 1 tiến trình thì giá trị
      cache không đổi). Tách riêng "kill switch check-live" thành 1 endpoint admin
      (`POST /tool-kill-switch`) hoá ra là giải quyết một vấn đề không tồn tại, mà lại mở thêm 1
      endpoint có thể tắt tool từ xa — rủi ro không đáng so với lợi ích. **Quyết định:** tắt 1 tool
      = sửa `TOOL_FLAGS` (hoặc catalog `status: "removed"`) + restart sidecar, không cần endpoint.
- [x] Thu hồi permission → **đã thoả bởi kiến trúc hiện có, không cần code thêm**: không tồn tại cache
      permission nào (context fetch mới ở đầu mỗi run), và backend check permission độc lập trên từng
      endpoint tool (`[HasPermission]`). Xem 17.7.
- [ ] **HOÃN sang Stage 10** — Resume run sau khi gỡ tool → plan chuyển `Drafting`, bước liên quan
      `invalid`, FE hiện cảnh báo. Cần `ChatRun.Plan`/plan chờ duyệt (Stage 10) — chưa tồn tại.
- [ ] **HOÃN sang Stage 10** — Duyệt plan sau 24h vẫn thực thi được (run token mới cấp lúc duyệt).
- [ ] **HOÃN sang Stage 10** — Permission được revalidate tại thời điểm duyệt plan, không dùng bản
      chụp cũ (chỉ có ý nghĩa khi có bước "duyệt" của Plan Mode).
- [x] **Thu hẹp phạm vi cho hiện tại (không hoãn)** — JWT gần hết hạn giữa run (E1, run tối đa 5 phút)
      → `ChatRunExecutor.EnsureFreshToken` tự ký lại (giữ nguyên claim, hạn mới) trước khi gọi sidecar,
      tái dùng `ITokenManagerService` có sẵn. **Không phải run token riêng, scope hẹp** như phương án A
      đề xuất ban đầu — phương án A đầy đủ (token chỉ dùng được `/internal/chat/tools/*`, không dùng
      chéo giữa các run) **hoãn sang Stage 10**, vì lý do thật để cần token độc lập là kịch bản "chờ
      duyệt 24h" — chưa tồn tại khi chưa có Plan Mode.
- [ ] **HOÃN sang Stage 10** — Run token không dùng được cho API ngoài `/internal/chat/tools/*`.
- [ ] **HOÃN sang Stage 10** — Run token của run A không dùng được cho run B.
- [x] `ChatRun.ModelUsed` ghi model thật từ `response_metadata` của LLM (event `run_meta`), so lệch
      với `AISetup:Model` → log ERROR.
- [ ] **Chưa làm (quy trình vận hành, không phải code)** — Eval hồi quy chạy theo lịch hằng tuần +
      `MODEL-CHANGELOG.md`. Cần lịch CI riêng, ghi chú follow-up thủ công.

### Test (đã viết, đang pass)

`AISidecar/tests/test_tool_lifecycle.py` (147/147 test toàn bộ sidecar pass):
- [x] `registry_fingerprint()` **tất định**; đổi `version` → đổi hash; tool `removed` bị bỏ qua khỏi hash.
- [x] Gọi tool bịa → gợi ý tên gần nhất, **không** crash run.
- [x] Gọi tool bịa lần 2 → `tools_disabled=True`, lượt kế tiếp không bind tool nào.
- [x] `status="removed"` có/không `replaced_by` → thông báo đúng từng trường hợp.
- [x] `sanitize_history` (`build_history_messages`) — lịch sử có tool call cũ → output không có
      `tool_calls` nào.
- [x] `verify_tool_contract` — tool thiếu endpoint → tự set `removed` + log ERROR, sidecar không crash;
      backend lỗi/không tới được → trả về mặc định an toàn, không crash.
- [x] `tool_flags` off theo snapshot đầu run (đổi env var có tác dụng sau khi restart sidecar).
- [x] Gọi tool thuộc module chưa nạp → tự nạp (tối đa 1 lần), thử lần 2 khi đã hết suất → báo không
      có quyền thay vì tự nạp tiếp.

`UnitTests/ChatRunExecutorTokenRefresh.cs` (2/2 pass) + `UnitTests/ChatRunExecutorTests.cs` (572/572 pass
toàn bộ backend):
- [x] Token còn nhiều thời gian → không mint lại.
- [x] Token còn dưới ngưỡng (5 phút — đúng thời lượng chạy tối đa 1 run) → mint lại trước khi gọi sidecar.
- [ ] **Chưa có / hoãn sang Stage 10** — run token không dùng được cho API ngoài
      `/internal/chat/tools/*`; run token của run A không dùng được cho run B; duyệt plan sau 24h cấp
      token mới + revalidate permission. Cả 3 cần Plan Mode.
