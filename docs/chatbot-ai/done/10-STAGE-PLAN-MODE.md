# Stage 10 — Plan Mode: tạo, sửa và duyệt kế hoạch

> Yêu cầu #3 · Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 3–4 ngày · Phụ thuộc: **Stage 8, 9**
> Mục tiêu: với yêu cầu phức tạp, AI **lập kế hoạch trước**, người dùng **sửa được kế hoạch**
> — kể cả **trong lúc kế hoạch đang được tạo** — rồi mới thực thi.

> ⚠️ **Nợ từ Stage 17 (Tool Lifecycle) — làm kèm khi có Plan Mode:**
> - **17.8** — Resume run / plan hết hiệu lực khi tool bị gỡ giữa lúc plan chờ duyệt: revalidate
>   bằng `registry_fingerprint()` (đã có sẵn từ 17.2) trước khi resume; tool cần đã gỡ → plan về
>   `Drafting`, bước liên quan `invalid`, phát event `plan_invalidated`.
> - **17.9 phương án A** — Run token riêng, scope hẹp (chỉ `/internal/chat/tools/*`, gắn `runId`,
>   không dùng chéo giữa các run). Hiện tại (chưa có Plan Mode) chỉ mới xử lý phần hẹp hơn: tự ký lại
>   JWT user khi gần hết hạn giữa run (`ChatRunExecutor.EnsureFreshToken`) — đủ cho run dài tối đa
>   5 phút, **không đủ** cho kịch bản "chờ duyệt 24h" của Stage 10. Khi duyệt plan, phải cấp run token
>   mới **và** revalidate permission tại thời điểm duyệt, không dùng permission đã chụp 24h trước.
>
> Xem chi tiết: [17-STAGE-TOOL-LIFECYCLE.md](done/17-STAGE-TOOL-LIFECYCLE.md) mục 17.8, 17.9.

Ví dụ:
> User: "Chuẩn bị báo cáo tồn kho quý này cho tôi"
> AI: *(sinh plan từng bước, stream ra màn hình)*
> 1. Lấy danh sách sản phẩm tồn kho thấp
> 2. Tính giá trị tồn kho theo danh mục
> 3. So sánh với quý trước
> 4. Tổng hợp thành báo cáo
>
> User *(sửa bước 3 khi AI còn đang viết bước 4)*: "So sánh với **cùng kỳ năm ngoái**"
> User: **Duyệt** → AI thực thi theo plan đã sửa.

---

## 10.1. Khi nào bật Plan Mode

| Chế độ | Kích hoạt | Hành vi |
|---|---|---|
| `Off` | Mặc định | Trả lời trực tiếp, không lập plan |
| `Auto` | Hệ thống tự phát hiện yêu cầu phức tạp | Lập plan → chờ duyệt → thực thi |
| `Always` | User bật toggle "Lập kế hoạch trước" | Luôn lập plan |

**Tiêu chí `Auto`** (đánh giá bằng model rẻ, xem Stage 14):
- Cần ≥ 3 lần gọi tool để trả lời, **hoặc**
- Có tool ghi dữ liệu (không chỉ đọc), **hoặc**
- Yêu cầu chứa từ khoá đa bước: "báo cáo", "phân tích", "tổng hợp", "so sánh", "lập kế hoạch",
  "kiểm tra toàn bộ".

> **Nguyên tắc bắt buộc:** mọi run có tool **ghi/sửa/xoá dữ liệu** đều phải qua Plan Mode và
> được duyệt tường minh. Đây cũng là một guardrail — xem [13-STAGE-GUARDRAILS.md](13-STAGE-GUARDRAILS.md).

---

## 10.2. Data model

`Domain/Entities/ChatPlan.cs`
```csharp
[Table("ChatPlan")]
public class ChatPlan : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("RunId")]
    [ForeignKey("Run")]
    public Guid RunId { get; set; }
    public ChatRun? Run { get; set; }

    [Required]
    [Column("SessionId")]
    public Guid SessionId { get; set; }

    /// <summary>Tăng mỗi lần plan bị sửa. Dùng cho optimistic concurrency.</summary>
    public int Version { get; set; } = 1;

    [Required]
    [Column("Status", TypeName = "nvarchar(30)")]
    public string Status { get; set; } = ChatPlanStatus.Drafting;

    /// <summary>Danh sách bước, JSON array của PlanStep.</summary>
    [Required]
    [Column("Steps", TypeName = "nvarchar(max)")]
    public string Steps { get; set; } = "[]";

    /// <summary>Ai chỉnh sửa lần cuối: "ai" hoặc "user".</summary>
    [Column("LastEditedBy", TypeName = "nvarchar(20)")]
    public string LastEditedBy { get; set; } = "ai";

    public DateTime? ApprovedAt { get; set; }
}
```

`Domain/Constants/ChatPlanStatus.cs`
```csharp
public static class ChatPlanStatus
{
    public const string Drafting  = "Drafting";   // AI đang sinh, user sửa được
    public const string Ready     = "Ready";      // AI sinh xong, chờ duyệt
    public const string Approved  = "Approved";   // đã duyệt, đang thực thi
    public const string Executing = "Executing";
    public const string Completed = "Completed";
    public const string Rejected  = "Rejected";   // user huỷ
}
```

### Cấu trúc `PlanStep` (JSON)

```jsonc
{
  "id": "s1",
  "order": 1,
  "title": "Lấy danh sách sản phẩm tồn kho thấp",
  "detail": "Gọi get_low_stock_products với ngưỡng 10",
  "expectedTools": ["get_low_stock_products"],
  "status": "pending",        // pending | running | done | failed | skipped
  "editedByUser": false,      // user đã sửa bước này chưa
  "result": null              // tóm tắt kết quả sau khi chạy
}
```

**Quan trọng — `editedByUser`:** khi user đã sửa một bước, AI **không được ghi đè** bước đó trong
các lần cập nhật plan tiếp theo. Đây là điểm mấu chốt để "sửa plan trong lúc đang tạo" không bị
AI đè lại (mục 10.4).

> **`expectedTools` không chỉ để hiển thị — nó là bảng phân bổ tool khi thực thi.**
> Chạy bước nào thì chỉ nạp tool của bước đó (2–3 tool thay vì 20), nên Plan Mode làm việc chọn
> tool **dễ đi**, không khó thêm. Khi user sửa bước bằng tiếng Việt, `expectedTools` phải được
> suy ra lại. Xem [20-STAGE-DYNAMIC-TOOL-SCOPING.md](20-STAGE-DYNAMIC-TOOL-SCOPING.md) mục 20.6.

### Migration
```powershell
./add-migration.ps1 AddChatPlan
```
Cho cả MySQL và PostgreSQL.

---

## 10.3. Loại event mới

| Type | Payload | Ý nghĩa |
|---|---|---|
| `plan_started` | `{"planId":"..."}` | AI bắt đầu lập plan |
| `plan_step_added` | `{"planId":"...","step":{...},"version":3}` | Thêm 1 bước (stream từng bước) |
| `plan_ready` | `{"planId":"...","version":5}` | AI lập xong, chờ duyệt |
| `plan_edited` | `{"planId":"...","version":6,"editedBy":"user"}` | Plan bị sửa |
| `plan_approved` | `{"planId":"...","version":6}` | User duyệt |
| `plan_rejected` | `{"planId":"..."}` | User huỷ |
| `plan_step_started` | `{"stepId":"s1"}` | Bắt đầu thực thi 1 bước |
| `plan_step_completed` | `{"stepId":"s1","status":"done","summary":"..."}` | Xong 1 bước |

Client tua lại từ event log (Stage 8) là dựng lại được toàn bộ plan → **không cần API riêng để lấy plan**,
nhưng vẫn nên có để đơn giản hoá FE:
```
GET /api/v1/manager-chat/runs/{runId}/plan
```

---

## 10.4. Sửa plan trong lúc AI đang tạo — vấn đề khó nhất

**Xung đột:** AI đang stream bước 4 thì user sửa bước 3. Nếu AI cập nhật cả plan → mất sửa của user.

### Giải pháp: quyền sở hữu theo từng bước + version

```
Quy tắc 1: AI chỉ được THÊM bước mới, không được sửa bước đã phát ra.
Quy tắc 2: Bước có editedByUser = true → AI không bao giờ được đụng vào.
Quy tắc 3: Mọi thay đổi đều tăng Version. Client gửi kèm Version mình đang thấy;
           lệch version → server trả 409, client tải lại plan rồi thử lại.
Quy tắc 4: User xoá bước → đánh dấu status = "skipped", KHÔNG xoá khỏi mảng
           (giữ id ổn định để AI không nhầm chỉ số).
```

### `UpdateChatPlanCommandHandler`

```
1. Kiểm tra plan thuộc về user, Status ∈ {Drafting, Ready}
2. Kiểm tra request.Version == plan.Version   → lệch thì trả Conflict
3. Áp thay đổi:
   - Sửa nội dung bước  → set title/detail, editedByUser = true
   - Thêm bước mới      → id mới, editedByUser = true
   - Xoá bước           → status = "skipped", editedByUser = true
   - Đổi thứ tự         → cập nhật order
4. plan.Version++, LastEditedBy = "user"
5. Append event plan_edited
6. Nếu AI đang lập plan → thông báo qua PendingSteering (tận dụng Stage 9)
   để lần sinh bước tiếp theo AI biết plan đã đổi
```

### Phía sidecar — AI phải đọc lại plan trước khi thêm bước

```python
async def plan_node(state: AgentState) -> dict:
    """Sinh plan từng bước, tôn trọng sửa đổi của user."""
    current = await backend.get_plan(state["run_id"])

    # Bước user đã sửa được đưa vào prompt như RÀNG BUỘC, không phải gợi ý
    locked = [s for s in current["steps"] if s["editedByUser"]]
    locked_text = "\n".join(
        f"- Bước {s['order']}: {s['title']} — {s['detail']}" for s in locked
    )

    prompt = render("system_plan_mode",
                    request=state["user_message"],
                    locked_steps=locked_text or "(chưa có)",
                    existing_count=len(current["steps"]))
    ...
```

Trong `app/prompts/system_plan_mode.md`:
```markdown
## Ràng buộc bắt buộc
Người dùng đã tự chỉnh sửa các bước sau. Bạn PHẢI giữ nguyên chúng,
KHÔNG được viết lại, không được đổi ý nghĩa, không được xoá:

{locked_steps}

Chỉ bổ sung các bước còn thiếu để hoàn thành yêu cầu.
```

---

## 10.5. Luồng duyệt & thực thi

```
plan_ready
   ↓
ChatRun.Status = AwaitingApproval        ← đã định nghĩa ở Stage 8
   ↓
[FE hiện plan panel — Sửa/Xoá/Thêm/kéo-thả vẫn qua nút, Duyệt/Huỷ qua chat (xem 10.9)]
   ↓
User gõ "duyệt" → POST /runs/{runId}/plan/chat → PlanChatClassifier → ApproveChatPlanCommand
   ↓
ChatRun.Status = Running
Plan.Status = Executing
   ↓
Agent chạy từng bước: plan_step_started → gọi tool → plan_step_completed
   ↓
plan_step_completed cho bước cuối → tổng hợp câu trả lời → run_completed
```

> **Đã đổi (Stage 10.9):** nút Duyệt/Huỷ trên PlanCard đã bỏ — user gõ chat để duyệt/huỷ, xem
> mục 10.9. Endpoint REST `plan/approve`/`plan/reject` vẫn còn nguyên, chỉ không còn được FE gọi
> trực tiếp từ nút bấm nữa mà được `SendPlanChatMessageCommand` gọi lại qua `ISender`.

### Chờ duyệt bao lâu?
- Run ở `AwaitingApproval` **không tính vào timeout 5 phút** của Stage 8.
- Sau **24 giờ** không duyệt → `Cancelled`, dọn bằng `OrphanedRunCleaner`.
- Người dùng thoát ra rồi quay lại (Stage 8) → thấy đúng plan đang chờ duyệt. **Đây là
  giao điểm quan trọng của Stage 8 và 10 — nhớ test.**

### Sửa plan sau khi đã duyệt?
- Bước `pending` → vẫn sửa được, plan `Version++`, agent đọc lại trước mỗi bước.
- Bước `running` / `done` → **không** sửa được.

---

## 10.6. Sidecar — graph có nhánh plan

> **Đã đổi so với thiết kế gốc (2026-07-31, sau khi trao đổi lại "muốn tập trung 1 DB"):**
> **không dùng `interrupt()`/checkpointer Postgres.** Lý do: toàn bộ dữ liệu cần nhớ để duyệt/resume
> (từng bước, trạng thái, ai sửa gì) **đã nằm sẵn trong bảng `ChatPlan` ở DB chính của backend**
> (SQL Server dev / MySQL, PostgreSQL production — tuỳ theo `DBContext` đang cấu hình, KHÔNG bắt
> buộc phải là Postgres). Dùng thêm một checkpointer Postgres riêng cho LangGraph là nhớ trùng lặp
> đúng dữ liệu đã có, mà lại vi phạm nguyên tắc ở mục 6: *"LLM không bao giờ chạm DB trực tiếp"*
> (Python vẫn phải tự mở connection tới Postgres cho checkpointer, bất kể `.NET` dùng DB nào).
>
> Thiết kế thật: khi plan sẵn sàng chờ duyệt, graph **kết thúc bình thường** (route `plan` → `END`,
> không pause) — giống hệt một run hoàn tất, `MemorySaver` là đủ vì không cần sống sót qua restart.
> Lúc user Duyệt, `.NET` enqueue lại run như một lời gọi **mới** tới sidecar; sidecar hỏi
> `GET /internal/chat/runs/{runId}/plan` — nếu `status == "Executing"` thì seed `plan_id` vào
> state ban đầu, route thẳng tới `execute_step` (bỏ qua `classify`/`plan`). Toàn bộ state cần thiết
> (lịch sử hội thoại, các bước plan) đọc lại tươi từ DB chính qua `BackendClient`, không phụ thuộc
> checkpoint cũ nào.

```python
def build_graph():
    graph = StateGraph(AgentState)
    graph.add_node("classify",         classify_node)        # cần plan không?
    graph.add_node("plan",             plan_node)            # sinh plan từng bước
    graph.add_node("absorb_steering",  absorb_steering_node) # Stage 9
    graph.add_node("execute_step",     execute_step_node)
    graph.add_node("call_model",       call_model_node)
    graph.add_node("call_tools",       call_tools_node)
    graph.add_node("step_completed",   step_completed_node)
    graph.add_node("summarize",        summarize_node)

    graph.set_entry_point("classify")
    graph.add_conditional_edges("classify", route_after_classify, {
        "plan": "plan",                    # yêu cầu mới, cần lập plan
        "execute_step": "execute_step",    # resume: plan_id đã seed từ backend (status=Executing)
        "absorb_steering": "absorb_steering",
    })

    graph.add_edge("plan", END)  # dừng ở đây — không interrupt, không checkpoint riêng
    graph.add_conditional_edges("execute_step", route_after_execute_step, {
        "call_model": "call_model", "summarize": "summarize",
    })
    graph.add_edge("step_completed", "execute_step")
    graph.add_edge("summarize", END)
    return graph.compile(checkpointer=MemorySaver())
```

`execute_step_node` chỉ được vào khi plan đã ở trạng thái `Executing` (đã qua `ApproveChatPlanCommand`
phía `.NET`) — set `plan_approved: True` ngay tại đây để tool guard (`check_tool_call`) cho phép tool
ghi dữ liệu, thay vì dựa vào giá trị do `await_approval_node` trả lại như bản thiết kế gốc.

---

## 10.7. Frontend — Plan Card

Component: `AnhEmMotor-Management/src/components/business/chat/PlanCard.vue`, mount trong
panel bên phải màn hình chat (`ChatDrawer.vue`), không còn nằm inline trong dòng chat — xem 10.9.

```
┌─────────────────────────────────────────────┐
│ 📋 Kế hoạch thực hiện          [Đang tạo…]  │
├─────────────────────────────────────────────┤
│ ⠿ 1. Lấy DS sản phẩm tồn kho thấp    ✎  ✕  │
│    💬 bình luận...                          │
│ ⠿ 2. Tính giá trị tồn theo danh mục   ✎  ✕  │
│ ⠿ 3. So sánh cùng kỳ năm ngoái  ✏️đã sửa ✎ ✕│
│ ⠿ 4. Tổng hợp báo cáo                 ✎  ✕  │
│    + Thêm bước                              │
├─────────────────────────────────────────────┤
│   💬 Gõ "duyệt" hoặc "huỷ" trong khung chat │
└─────────────────────────────────────────────┘
```

Yêu cầu:
- Bước mới **stream vào từng cái** (nhận `plan_step_added`) — user thấy plan lớn dần.
- Sửa inline: click ✎ → textarea → blur/Enter thì lưu.
- Kéo thả đổi thứ tự (`⠿`), dùng `vuedraggable`.
- Bước user đã sửa có badge `✏️ đã sửa` để phân biệt.
- Khi đang thực thi: mỗi bước hiện spinner / ✓ / ✗ theo `plan_step_*` event.
- Gửi `version` trong mọi request sửa; nhận 409 → tải lại plan, hiện toast
  "Kế hoạch vừa được cập nhật, vui lòng xem lại".
- ~~Nút Duyệt/Huỷ~~ — **đã bỏ (Stage 10.9)**, thay bằng gõ chat. Mỗi bước có thêm ô bình luận
  riêng, có thể để nhiều bình luận/bước.

### API cho FE
```
GET   /api/v1/manager-chat/runs/{runId}/plan
PATCH /api/v1/manager-chat/runs/{runId}/plan          { version, operations: [...] }
POST  /api/v1/manager-chat/runs/{runId}/plan/chat     { content, targetStepId? }   ← Stage 10.9
```
`plan/approve` và `plan/reject` (REST) vẫn tồn tại nguyên vẹn cho tương thích/gọi nội bộ, chỉ
không còn được FE gọi trực tiếp — xem 10.9.

---

## 10.8. Rủi ro

| Rủi ro | Giảm thiểu |
|---|---|
| AI ghi đè bước user vừa sửa | `editedByUser` + prompt ràng buộc + test riêng |
| Xung đột version khi user sửa nhanh | Optimistic concurrency + 409 + reload |
| Plan quá dài, user không đọc | Giới hạn 8 bước; nhiều hơn thì AI phải gộp |
| User không duyệt, run treo | Timeout 24h → Cancelled |
| Plan chờ duyệt bị mất khi restart | `ChatPlan` đã nằm ở DB chính backend (không phải checkpoint LangGraph) — restart không mất; lúc duyệt, sidecar dựng lại state từ DB, không phụ thuộc `MemorySaver` sống hay chết |
| AI lập plan sai rồi thực thi luôn | Bắt buộc duyệt với mọi tool ghi dữ liệu |

---

## 10.9. Vá 2 bug thật + chat-driven Plan Mode (2026-08-03)

Sau khi vận hành thật, phát hiện 2 bug và đổi UX theo yêu cầu người dùng.

### Bug 1 — `allowed_tool_names` rỗng khi thực thi plan step dù user đủ quyền

Root cause: `route_after_classify` route thẳng vào `execute_step` khi resume, bỏ qua
`absorb_steering_node` — node duy nhất tính `scoped_modules`. Cứu cánh
`current_plan_step["expectedTools"]` cũng gãy vì tên tool do LLM tự sinh ở `plan_node` chưa từng
được đối chiếu `load_tool_specs()` trước khi lưu.

**Fix:** `plan_node` (`AISidecar/app/agents/manager_agent.py`) lọc `TOOLS:` qua
`load_tool_specs()` (chỉ giữ tên có thật, `status == "active"`) trước khi gọi `add_plan_step` —
đúng chỗ ghi duy nhất của `expectedTools`, không đụng routing.

### Bug 2 — PlanCard báo nhầm "mất kết nối" khi chờ duyệt, nhân đôi sau khi duyệt

Root cause: sau `plan_ready` không còn `run_heartbeat` nào (graph đã kết thúc, route `plan→END`).
Watchdog FE 45s chỉ reset bằng `run_heartbeat` → chắc chắn nổ, xoá `activePlans[sessionId]`.

**Fix:** `ChatDrawer.vue` — `clearWatchdog` ở `plan_ready`/khi resume vào `AwaitingApproval`,
`armWatchdog` lại ở `plan_approved`. Bug nhân đôi tự hết sau khi bỏ cơ chế push-plan-vào-`messages`
(xem dưới).

### Chat-driven: bỏ nút Duyệt/Huỷ, panel bên phải, bình luận theo bước

**Panel:** `activePlans` đổi từ `Record<sessionId, ChatMessage>` (bọc plan như 1 "tin nhắn giả"
trong dòng chat) sang thẳng `Record<sessionId, ChatPlanDto>`. `PlanCard` mount trong cột riêng bên
phải (`ChatDrawer.vue`, toggle mở/đóng bằng nút "📋 Kế hoạch" ở header), không còn push vào mảng
`messages` — nhờ vậy bug nhân đôi PlanCard khi resume cũng hết theo, không cần dedupe thủ công nữa.

**Duyệt/Huỷ qua chat, không qua nút:** mọi tin nhắn gõ trong lúc `Plan.Status ∈ {Drafting, Ready}`
đi qua `POST runs/{runId}/plan/chat` (`SendPlanChatMessageCommand`) thay vì `SendSteering` — vốn cố
ý từ chối `AwaitingApproval` (coi run đã kết thúc) và trước đây âm thầm tạo hẳn 1 run mới không
liên quan gì tới plan.

`SendPlanChatMessageCommandHandler` chỉ điều phối, KHÔNG viết lại nghiệp vụ đã có:
1. `PlanChatClassifier.Classify(content)` — khớp CHÍNH XÁC (không phải substring) một tập từ khoá
   duyệt (`"duyệt"`, `"đồng ý"`, `"ok"`...) / huỷ (`"huỷ"`, `"không"`, `"thôi"`...). Khớp thì gọi
   lại `ApproveChatPlanCommand`/`RejectChatPlanCommand` có sẵn qua `ISender` — mọi
   ownership/version-conflict/permission vẫn do 2 handler đó tự kiểm tra.
2. Không khớp + có `targetStepId` (gõ vào đúng ô bình luận của 1 bước trên PlanCard) → ghép thẳng
   operation `{"type":"comment", stepId, comment}`, KHÔNG cần LLM (đã rõ ràng).
3. Không khớp + không có `targetStepId` (chat tự do, không gắn bước nào) → gọi sidecar
   `POST /plan/interpret` (`app/api/v1/chat.py`) — LLM diễn giải free-text thành
   `operations` (edit/add/remove/reorder/comment), tái dùng nguyên mẫu `PydanticOutputParser` đã
   có ở `search_products.py` và hàm `infer_step_tools()` có sẵn (trước đó chưa được gọi ở đâu) để
   suy `expectedTools` cho bước vừa sửa. Trả `intent: "unclear"` nếu không đủ rõ — không tự sửa liều.
4. Cả 2 nhánh trên đều KẾT THÚC bằng gọi lại `UpdateChatPlanCommand` có sẵn — không có đường ghi
   plan nào đi tắt qua handler mới.

**Bình luận theo bước:** `PlanStepDto` thêm `Comments: List<PlanStepCommentDto>?` (nullable, JSON
blob nên không cần migration — plan cũ deserialize ra `null`, chỗ ghi tự `?? []`). Nhiều bình luận
tích luỹ trên cùng 1 bước, mỗi lần gọi `operation "comment"` append thêm, không ghi đè.

### File mới/sửa chính
- `AISidecar/app/agents/manager_agent.py` — lọc tool trong `plan_node`.
- `AISidecar/app/api/v1/chat.py`, `app/schemas/plan_chat.py`, `app/prompts/plan_chat_intent.md` —
  endpoint `/plan/interpret`.
- `Application/Features/ManagerChat/Commands/SendPlanChatMessage/*` — Command/Handler/Classifier.
- `Application/DTOs/Chat/PlanStepCommentDto.cs`, `PlanChatResultDto.cs`, `PlanChatInterpretationDto.cs`.
- `ISidecarStreamClient`/`SidecarStreamClient` — `InterpretPlanChatAsync`.
- `ChatDrawer.vue`, `PlanCard.vue`, `chat.api.ts` — panel, bỏ nút, ô bình luận, `sendPlanChat`.

---

## Definition of Done — Stage 10

- [ ] Migration `ChatPlan` chạy được trên cả MySQL và PostgreSQL.
- [ ] Yêu cầu phức tạp → tự động vào Plan Mode; yêu cầu đơn giản → trả lời thẳng.
- [ ] Plan stream ra **từng bước một**, không chờ sinh xong mới hiện.
- [ ] **Sửa bước 2 trong lúc AI đang viết bước 4 → sửa đổi được giữ nguyên**, AI không ghi đè.
- [ ] Thêm / xoá / kéo đổi thứ tự bước đều hoạt động.
- [ ] Sửa với version cũ → nhận 409, FE tải lại plan và thông báo.
- [ ] Duyệt → thực thi đúng plan đã sửa (không phải plan gốc của AI).
- [ ] Đang chờ duyệt → thoát ra, restart backend, vào lại → plan vẫn còn nguyên, duyệt được.
- [ ] Mọi tool ghi dữ liệu đều bị chặn nếu chưa có plan được duyệt.
- [ ] Không duyệt sau 24h → run tự huỷ.
- [ ] (10.9) Gõ "duyệt"/"huỷ" trong chat lúc plan `Ready` → thực thi/huỷ đúng, không tạo run mới.
- [ ] (10.9) Bình luận vào 1 bước cụ thể → xuất hiện đúng bước đó, cộng dồn được nhiều bình luận.
- [ ] (10.9) Chat tự do không khớp keyword → sidecar diễn giải đúng thành sửa bước tương ứng.

### Test

`UnitTests/ManagerChatPlan.cs`:
- [ ] Sửa với `version` cũ → `Conflict`, plan **không** bị đổi.
- [ ] AI cập nhật plan khi có bước `editedByUser=true` → bước đó **giữ nguyên từng ký tự**.
- [ ] Xoá bước → `status="skipped"`, **id các bước khác không đổi**.
- [ ] Duyệt plan của user khác → NotFound.
- [ ] Duyệt khi `Status=Drafting` → bị từ chối.
- [ ] Sửa bước đang `running`/`done` → bị từ chối; bước `pending` → cho phép.
- [ ] Plan > 8 bước → bị từ chối, có thông báo rõ.
- [ ] (10.9) `PlanChatClassifier` khớp đúng từ khoá duyệt/huỷ; `SendPlanChatMessageCommandHandler`
      route đúng approve/reject/update; bình luận cộng dồn không ghi đè.

`AISidecar/tests/test_plan.py`:
- [ ] `plan_node` đưa `locked_steps` vào prompt đúng định dạng khi có bước user sửa.
- [ ] (10.9) `plan_node` lọc tên tool bịa trước khi lưu `expectedTools`.
- [ ] (10.9) `call_tools_node` chặn đúng tool ghi khi `plan_approved=False`, cho qua khi `True`.

`AISidecar/tests/test_plan_chat.py` (mới, Stage 10.9):
- [ ] `/plan/interpret` suy đúng `expected_tools` cho operation `edit`; operation `comment` không
      gọi `infer_step_tools`; LLM lỗi → trả `intent: "unclear"`.
