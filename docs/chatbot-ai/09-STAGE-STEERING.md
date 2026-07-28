# Stage 9 — Steering: chat tiếp khi AI đang suy nghĩ

> Yêu cầu #2 · Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 2–3 ngày · Phụ thuộc: **Stage 8**
> Mục tiêu: đang lúc AI suy nghĩ, người dùng vẫn gửi được tin nhắn tiếp và **thay đổi được
> thông tin** mà AI đang xử lý — không phải chờ xong rồi mới nói lại từ đầu.

Ví dụ thực tế:
> User: "Xem doanh thu tháng này"
> *(AI đang gọi tool...)*
> User: "À nhầm, tháng trước cơ"
> → AI phải chuyển sang tháng trước, **không** trả lời tháng này rồi mới trả lời tháng trước.

---

## 9.1. Ba chế độ xử lý — chọn theo ngữ cảnh

| Chế độ | Khi nào | Hành vi |
|---|---|---|
| **Queue** (xếp hàng) | Tin nhắn bổ sung, không mâu thuẫn ("thêm cả số đơn hàng nữa") | Chèn vào state ở ranh giới bước tiếp theo, AI xử lý luôn trong cùng run |
| **Interrupt** (ngắt & đổi hướng) | Tin nhắn sửa/phủ định thông tin cũ ("à nhầm, tháng trước") | Dừng bước hiện tại, gộp ngữ cảnh, chạy lại từ điểm checkpoint |
| **Restart** (chạy lại) | Đổi hoàn toàn chủ đề | Huỷ run cũ (`Cancelled`), tạo run mới |

> **Ba chế độ này cũng quyết định tập tool được nạp lại như thế nào:** `queue` → **hợp** scope,
> `interrupt` → **thay** scope, `restart` → scope mới hoàn toàn.
> Xem [20-STAGE-DYNAMIC-TOOL-SCOPING.md](20-STAGE-DYNAMIC-TOOL-SCOPING.md) mục 20.7 —
> `absorb_steering_node` là nơi thực hiện cả hai việc.

**Quyết định thiết kế:** không bắt user chọn. Hệ thống tự phân loại (mục 9.4), nhưng luôn có
nút để user ép "Dừng và hỏi lại" (= Restart).

> **Mặc định an toàn:** nếu phân loại không chắc chắn → chọn **Queue**. Queue không bao giờ làm
> mất công việc đang chạy; Interrupt thì có.

---

## 9.2. Data model bổ sung

Thêm vào `ChatRun` (mở rộng entity ở Stage 8):

```csharp
/// <summary>Tin nhắn steering đang chờ được nạp vào agent, dạng JSON array.</summary>
[Column("PendingSteering", TypeName = "nvarchar(max)")]
public string PendingSteering { get; set; } = "[]";
```

**Migration** (cột `ChatRun.PendingSteering`, `ChatMessage.IsSteering`, `ChatMessage.RunId`) —
tạo cho **cả** MySQL và PostgreSQL:
```powershell
./add-migration.ps1 AddSteeringSupport
```

> **Vì sao không tạo bảng riêng:** steering là dữ liệu tạm trong vòng đời 1 run, số lượng rất nhỏ
> (1–3 tin), luôn đọc/ghi cùng lúc với `ChatRun`. Cột JSON đơn giản hơn và tránh thêm 1 join.
> Nếu sau này cần audit đầy đủ thì đã có `ChatRunEvent` với type `steering_received`.

Loại event mới:

| Type | Payload | Ý nghĩa |
|---|---|---|
| `steering_received` | `{"content":"...","mode":"queue"}` | Đã nhận, đang chờ |
| `steering_applied` | `{"content":"...","mode":"interrupt"}` | Đã nạp vào agent |
| `run_redirected` | `{"reason":"user_correction"}` | Agent đổi hướng |

`ChatMessage` cũng cần phân biệt tin nhắn steering với tin nhắn thường:
```csharp
/// <summary>Tin nhắn được gửi khi run đang chạy (steering).</summary>
public bool IsSteering { get; set; }

/// <summary>Run mà tin nhắn này gắn vào (null với tin nhắn khởi tạo run).</summary>
public Guid? RunId { get; set; }
```

---

## 9.3. Luồng backend

### Hub method mới

```csharp
/// <summary>Gửi tin nhắn khi có run đang chạy.</summary>
public async Task<SteeringResultDto> SendSteering(Guid runId, string content)
{
    var userId = ParseUserId();
    return await sender.Send(new SendSteeringMessageCommand(runId, content, userId));
}
```

FE quyết định gọi `StartRun` hay `SendSteering` dựa trên việc có run active hay không.
Backend vẫn phải tự kiểm tra: `StartRun` khi đã có run `Running` cho session → trả lỗi rõ ràng.

### `SendSteeringMessageCommandHandler`

```
1. Xác thực run tồn tại, thuộc về user, đang ở trạng thái Running/AwaitingApproval
   → nếu run đã kết thúc: KHÔNG lỗi, mà tự động tạo run mới (UX mượt hơn)
2. Lưu ChatMessage { Role=User, IsSteering=true, RunId=runId }
3. Phân loại chế độ (mục 9.4) → queue | interrupt | restart
4. Append event steering_received
5. Theo chế độ:
   - queue     → append vào ChatRun.PendingSteering
   - interrupt → append vào PendingSteering + set cờ InterruptRequested, huỷ bước hiện tại
   - restart   → CancelChatRun(runId) rồi StartChatRun(newContent)
6. Trả về { mode, runId }
```

**Race condition quan trọng:** run có thể kết thúc **đúng lúc** steering đang được xử lý.
Xử lý bằng optimistic concurrency: `ExecuteUpdateAsync` với điều kiện `WHERE Status = 'Running'`;
nếu 0 dòng bị ảnh hưởng → run đã xong → chuyển sang tạo run mới.

---

## 9.4. Phân loại chế độ

### Tầng 1 — Luật (rẻ, chạy trước)

```python
CORRECTION_MARKERS = [
    "à nhầm", "à quên", "nhầm rồi", "sai rồi", "không phải",
    "ý tôi là", "ý mình là", "sửa lại", "đổi thành", "thay vì",
    "khoan", "dừng lại", "bỏ qua",
]

def quick_classify(text: str) -> str | None:
    lowered = text.lower().strip()
    if any(marker in lowered for marker in CORRECTION_MARKERS):
        return "interrupt"
    if lowered in {"dừng", "stop", "thôi", "huỷ"}:
        return "restart"
    return None      # không chắc → để tầng 2 quyết định
```

### Tầng 2 — LLM nhỏ (chỉ khi tầng 1 không quyết được)

Dùng model rẻ, prompt cực ngắn, `max_tokens` thấp:

```
Câu hỏi gốc: {original}
Tin nhắn mới: {steering}

Tin nhắn mới BỔ SUNG hay THAY THẾ thông tin trong câu hỏi gốc?
Trả về đúng một từ: BO_SUNG hoặc THAY_THE
```

`BO_SUNG` → queue · `THAY_THE` → interrupt

> **Chi phí:** ~50 token/lần, chỉ chạy khi user thực sự steering (hiếm). Chấp nhận được.
> Nếu LLM lỗi/timeout 2s → mặc định `queue`.

---

## 9.5. Sidecar — nạp steering vào agent

Đây là lý do Stage 8 cần **LangGraph checkpointer**: state của agent được lưu theo `thread_id = run_id`,
nên có thể sửa state giữa chừng.

### Kiến trúc

```python
# app/agents/manager_agent.py
from langgraph.graph import StateGraph, END

def build_graph(tools, llm):
    graph = StateGraph(AgentState)
    graph.add_node("absorb_steering", absorb_steering_node)   # ← node mới
    graph.add_node("call_model", call_model_node)
    graph.add_node("call_tools", call_tools_node)

    graph.set_entry_point("absorb_steering")
    graph.add_edge("absorb_steering", "call_model")
    graph.add_conditional_edges("call_model", route_after_model, {
        "tools": "call_tools",
        "end": END,
    })
    # Sau mỗi vòng tool, QUAY LẠI absorb_steering — đây là "ranh giới bước"
    graph.add_edge("call_tools", "absorb_steering")
    return graph.compile(checkpointer=checkpointer)
```

**`absorb_steering_node`** là điểm chèn duy nhất — chạy trước mỗi lượt gọi model:

```python
async def absorb_steering_node(state: AgentState) -> dict:
    """Kéo tin nhắn steering đang chờ từ backend và nạp vào hội thoại."""
    pending = await backend.pull_pending_steering(state["run_id"])
    if not pending:
        return {}

    new_messages = []
    for item in pending:
        if item["mode"] == "interrupt":
            # Nói rõ cho model biết đây là đính chính, không phải câu hỏi mới
            new_messages.append(HumanMessage(content=(
                f"[ĐÍNH CHÍNH TỪ NGƯỜI DÙNG] {item['content']}\n"
                f"Hãy điều chỉnh theo thông tin mới này. "
                f"Bỏ qua phần công việc đã làm nếu không còn phù hợp, "
                f"và KHÔNG trả lời cho yêu cầu cũ nữa."
            )))
        else:
            new_messages.append(HumanMessage(content=(
                f"[BỔ SUNG TỪ NGƯỜI DÙNG] {item['content']}"
            )))

    return {
        "messages": new_messages,
        "steering_applied": True,
    }
```

**Endpoint mới ở .NET** để sidecar kéo steering (và xoá khỏi hàng chờ nguyên tử):
```
POST /internal/chat/runs/{runId}/pull-steering  →  [{content, mode}]
```
Phải là thao tác **read-and-clear nguyên tử** (transaction), tránh nạp trùng.

### Ngắt tool đang chạy (chế độ interrupt)

Nếu đang giữa lúc gọi tool mà nhận `interrupt`:
- **Không** giết tool đang chạy (nó có thể là ghi dữ liệu — xem Stage 13).
- Để tool chạy nốt, **bỏ qua kết quả**, rồi vào `absorb_steering`.
- Tool chỉ-đọc thì bỏ kết quả là an toàn tuyệt đối.

---

## 9.6. Frontend

### Trạng thái nút gửi

| Trạng thái run | Placeholder ô nhập | Nút chính |
|---|---|---|
| Không có run | "Nhập tin nhắn..." | Gửi |
| Đang chạy | "Gửi thêm thông tin hoặc đính chính..." | Gửi (steering) |
| Đang chạy | — | Nút phụ: **Dừng** |

**Không được disable ô nhập khi AI đang chạy** — đó chính là điểm của Stage này.

### Hiển thị

- Tin nhắn steering render **thu nhỏ, thụt vào**, gắn nhãn nhỏ:
  - Chế độ queue → `＋ đã ghi nhận, AI sẽ xử lý luôn`
  - Chế độ interrupt → `↻ AI đang điều chỉnh theo yêu cầu mới`
- Khi nhận event `run_redirected` → hiện dòng phân cách trong bubble AI:
  `— Đã chuyển hướng theo đính chính —`
- Nếu run vừa kết thúc lúc user bấm gửi → hiển thị như tin nhắn thường của run mới,
  **không hiện lỗi**.

> ⚠️ **Khoảng trống giữa `steering_received` và `steering_applied`.** Agent chỉ nạp steering ở
> ranh giới bước — nếu đang giữa một tool chậm, user thấy tin nhắn mình gửi mà AI như phớt lờ.
> Bắt buộc hiển thị trạng thái chờ, và sau 20 giây thì hiện nút **Dừng và hỏi lại**.
> Chi tiết ở [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.6.

### Xử lý gõ nhanh

Debounce không phù hợp ở đây (user cần gửi ngay). Thay vào đó:
- Gửi steering liên tiếp trong < 1s → gom lại thành 1 tin nhắn ở FE trước khi gửi.
- Giới hạn 5 steering / run → vượt thì gợi ý "Dừng và hỏi lại từ đầu".

---

## 9.7. Rủi ro

| Rủi ro | Giảm thiểu |
|---|---|
| Agent bối rối vì quá nhiều đính chính | Giới hạn 5 steering/run; prompt nói rõ ưu tiên thông tin mới nhất |
| Steering đến sau khi run xong → user tưởng bị nuốt | Tự động tạo run mới, không báo lỗi |
| Nạp steering trùng 2 lần | `pull-steering` phải nguyên tử (transaction + clear) |
| Interrupt lúc đang gọi tool ghi dữ liệu | Không huỷ tool ghi; xem Stage 13 (tool ghi cần xác nhận trước) |
| Chi phí phân loại bằng LLM | Tầng 1 bằng luật xử lý phần lớn trường hợp |

---

## Definition of Done — Stage 9

- [ ] Ô nhập **không** bị khoá khi AI đang chạy.
- [ ] Gửi "thêm cả số đơn hàng nữa" khi đang chạy → AI trả lời gộp cả hai trong cùng một run.
- [ ] Gửi "à nhầm, tháng trước cơ" khi đang chạy → AI chuyển sang tháng trước, **không** trả lời tháng này.
- [ ] Gửi steering đúng lúc run vừa kết thúc → tự tạo run mới, không hiện lỗi.
- [ ] Tin nhắn steering hiển thị khác biệt và được lưu vào lịch sử với `IsSteering = true`.
- [ ] `pull-steering` gọi 2 lần liên tiếp → lần 2 trả rỗng (không nạp trùng).
- [ ] Test: 3 steering gửi trong 500ms → không mất, không trùng.
- [ ] Thoát ra vào lại (Stage 8) giữa lúc có steering đang chờ → trạng thái vẫn đúng.

### Test

`UnitTests/ManagerChatSteering.cs` + `AISidecar/tests/test_steering.py`:
- [ ] `quick_classify` — bảng tham số: "à nhầm" → interrupt, "thêm cả" → queue,
      "dừng" → restart, chuỗi rỗng → None.
- [ ] Tầng 2 LLM lỗi/timeout → mặc định `queue` (**không** interrupt — mất việc đang chạy).
- [ ] **Race:** `pull-steering` gọi đồng thời 2 lần → tổng số item trả về đúng bằng số đã ghi,
      không trùng không mất (test transaction).
- [ ] **Race:** steering tới đúng lúc `ExecuteUpdateAsync` thấy `Status != 'Running'`
      → tạo run mới, trả `mode` hợp lệ, không ném exception.
- [ ] 3 steering gửi trong 500ms → FE gom thành 1, backend nhận đúng 1.
- [ ] Vượt 5 steering/run → trả lỗi có hướng dẫn, không âm thầm bỏ qua.
- [ ] `absorb_steering_node` với `pending` rỗng → trả `{}`, không tạo message thừa.
