# Stage 8 — Run Engine: chạy nền & khôi phục trạng thái

> Yêu cầu #4 · Ưu tiên: 🔴 Cao · Ước lượng: 3–4 ngày · Phụ thuộc: **Stage 1, 7**
> Mục tiêu: người dùng thoát ra giữa lúc AI đang chạy, quay lại thấy **đúng trạng thái đang chạy**
> và xem tiếp được phần đã sinh ra trong lúc vắng mặt.

Đây là Stage **nền tảng kiến trúc** — Stage 9 (steering), 10 (plan mode), 11 (hiển thị suy nghĩ)
đều xây trên nó. Làm đúng ở đây thì 3 Stage sau rất nhẹ.

---

## 8.1. Vì sao kiến trúc hiện tại không làm được

`StreamManagerChatMessageCommandHandler` chạy **bên trong** lời gọi SignalR hub:

```csharp
public async IAsyncEnumerable<string> Handle(..., [EnumeratorCancellation] CancellationToken ct)
```

- `ct` gắn với **kết nối của client**. Client đóng tab → `ct` cancel → vòng lặp `break` →
  AI dừng giữa chừng, phần đang sinh mất trắng.
- Không có thực thể nào ghi lại "đang chạy". Reload trang → FE chỉ thấy lịch sử tin nhắn đã lưu,
  không biết có run nào đang treo.
- Stream là chuỗi ký tự thô, không tua lại được từ vị trí bất kỳ.

**Kết luận:** phải tách **vòng đời của run** ra khỏi **vòng đời của kết nối**.

---

## 8.2. Kiến trúc mục tiêu

```
Client gửi tin nhắn
   ↓
Hub: StartRun(sessionId, content)  → tạo ChatRun (Status=Running), trả runId NGAY
   ↓                                              (không chờ AI)
ChatRunExecutor (BackgroundService)
   ↓ chạy độc lập với kết nối client
   ↓ mỗi sự kiện → ghi vào bảng ChatRunEvent (append-only, có Seq tăng dần)
   ↓ đồng thời publish vào in-memory channel
   ↓
Hub: SubscribeRun(runId, afterSeq)
   ├─ đọc ChatRunEvent WHERE Seq > afterSeq  → replay phần đã lỡ
   └─ rồi nối vào channel để nhận tiếp realtime
```

**Ý tưởng cốt lõi:** event log append-only. Client chỉ cần nhớ `lastSeq` đã nhận,
lúc nào kết nối lại cũng tua đúng chỗ. Đây cũng chính là cơ chế cho Stage 11.

---

## 8.3. Data model

### Entity mới: `ChatRun`

`Domain/Entities/ChatRun.cs`
```csharp
[Table("ChatRun")]
public class ChatRun : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("SessionId")]
    [ForeignKey("Session")]
    public Guid SessionId { get; set; }
    public ChatSession? Session { get; set; }

    [Required]
    [Column("Status", TypeName = "nvarchar(30)")]
    public string Status { get; set; } = ChatRunStatus.Pending;

    /// <summary>Nội dung người dùng khởi tạo run.</summary>
    [Required]
    [Column("UserMessage", TypeName = "nvarchar(max)")]
    public string UserMessage { get; set; } = string.Empty;

    /// <summary>Nội dung AI đã sinh (cập nhật dần, dùng để khôi phục và lưu cuối run).</summary>
    [Column("PartialOutput", TypeName = "nvarchar(max)")]
    public string PartialOutput { get; set; } = string.Empty;

    /// <summary>Seq lớn nhất đã ghi — tránh phải MAX() mỗi lần append.</summary>
    public long LastSeq { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [Column("ErrorCode", TypeName = "nvarchar(100)")]
    public string? ErrorCode { get; set; }

    /// <summary>Instance nào đang chạy run này (chuẩn bị cho multi-instance).</summary>
    [Column("OwnerInstanceId", TypeName = "nvarchar(100)")]
    public string? OwnerInstanceId { get; set; }

    /// <summary>Heartbeat — dùng để phát hiện run mồ côi khi app crash.</summary>
    public DateTime? HeartbeatAt { get; set; }

    public ICollection<ChatRunEvent> Events { get; set; } = [];
}
```

`Domain/Constants/ChatRunStatus.cs`
```csharp
public static class ChatRunStatus
{
    public const string Pending   = "Pending";     // đã tạo, chưa bắt đầu
    public const string Running   = "Running";     // đang chạy
    public const string Completed = "Completed";   // xong bình thường
    public const string Cancelled = "Cancelled";   // user bấm dừng
    public const string Failed    = "Failed";      // lỗi
    public const string Orphaned  = "Orphaned";    // app chết giữa chừng, được dọn khi khởi động lại
    public const string AwaitingApproval = "AwaitingApproval"; // Stage 10 — chờ duyệt plan
}
```

### Entity mới: `ChatRunEvent`

`Domain/Entities/ChatRunEvent.cs`
```csharp
[Table("ChatRunEvent")]
public class ChatRunEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("RunId")]
    [ForeignKey("Run")]
    public Guid RunId { get; set; }
    public ChatRun? Run { get; set; }

    /// <summary>Số thứ tự tăng dần trong phạm vi 1 run. Client dùng để tua.</summary>
    [Required]
    public long Seq { get; set; }

    [Required]
    [Column("Type", TypeName = "nvarchar(40)")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Payload JSON — hình dạng tuỳ theo Type.</summary>
    [Column("Payload", TypeName = "nvarchar(max)")]
    public string Payload { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Index bắt buộc** (trong `ApplicationDBContext.OnModelCreating`):
```csharp
modelBuilder.Entity<ChatRunEvent>()
    .HasIndex(e => new { e.RunId, e.Seq })
    .IsUnique();

modelBuilder.Entity<ChatRun>()
    .HasIndex(r => new { r.SessionId, r.Status });
```

### Bảng loại event

| Type | Payload | Stage |
|---|---|---|
| `run_started` | `{}` | 8 |
| `text_delta` | `{"content":"..."}` | 8 |
| `error` | `{"code":"llm_error","message":"..."}` | 8 |
| `run_completed` | `{"finishReason":"stop"}` | 8 |
| `run_cancelled` | `{}` | 8 |
| `steering_received` | `{"content":"..."}` | 9 |
| `plan_draft` / `plan_updated` / `plan_approved` | xem Stage 10 | 10 |
| `thinking` | `{"content":"..."}` | 11 |
| `tool_start` | `{"name":"...","argsPreview":"...","callId":"..."}` | 11 |
| `tool_end` | `{"callId":"...","status":"ok","summary":"...","resultPreview":"..."}` | 11 |
| `guardrail_blocked` | `{"reason":"..."}` | 13 |
| `guardrail_tool_budget` | `{"loaded":20,"dropped":["..."]}` | 13.3b |
| `module_loaded` | `{"module":"inventory"}` | 17.3 |
| `registry_changed` | `{"from":"abc123","to":"def456"}` | 17.8 |
| `plan_invalidated` | `{"planId":"...","unavailableTools":["..."]}` | 17.8 |
| `run_heartbeat` | `{}` — phát mỗi 15s | 18.4 |

> ⚠️ **Bảng này là danh sách canonical duy nhất.** Stage nào thêm loại event mới thì **phải**
> bổ sung vào đây, không định nghĩa rải rác — nếu không FE không biết cần xử lý những gì.

> **Nguyên tắc tương thích:** thêm loại event mới **không được** phá client cũ.
> FE phải **bỏ qua im lặng** mọi `Type` lạ (không log lỗi, không vỡ giao diện) — điều này cho phép
> deploy backend trước FE. Viết test cho tình huống này ở FE.

### Migration

```powershell
./add-migration.ps1 AddChatRunAndRunEvent
```
Nhớ tạo cho **cả** `MySqlMigrations` và `PostgreSqlMigrations` như các migration hiện có.

**Cascade:** xoá `ChatSession` → xoá `ChatRun` → xoá `ChatRunEvent`.

---

## 8.4. Backend — các thành phần mới

```
Application/Features/ManagerChat/
  Commands/
    StartChatRun/                    # tạo run, đẩy vào queue, trả runId ngay
    CancelChatRun/                   # user bấm dừng
  Queries/
    GetActiveChatRun/                # FE gọi khi mở lại session
    GetChatRunEvents/                # tua từ afterSeq

Application/Interfaces/Services/
  IChatRunQueue.cs                   # hàng đợi run chờ chạy
  IChatRunEventBus.cs                # pub/sub realtime trong process
  IChatRunWriter.cs                  # ghi event (append + tăng Seq an toàn)

Infrastructure/Services/Ai/Runs/
  ChatRunQueue.cs                    # Channel<Guid>
  ChatRunEventBus.cs                 # Channel per runId
  ChatRunWriter.cs
  ChatRunExecutor.cs                 # BackgroundService — trái tim của Stage này
  OrphanedRunCleaner.cs              # dọn run mồ côi lúc khởi động + định kỳ
```

### `ChatRunExecutor` — khung sườn

```csharp
public class ChatRunExecutor(
    IChatRunQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<ChatRunExecutor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Chạy nhiều run song song, giới hạn bằng SemaphoreSlim
        await foreach (var runId in queue.ReadAllAsync(stoppingToken))
        {
            _ = Task.Run(() => ExecuteRunSafeAsync(runId, stoppingToken), stoppingToken);
        }
    }

    private async Task ExecuteRunSafeAsync(Guid runId, CancellationToken appStopping)
    {
        // QUAN TRỌNG: dùng scope riêng — DbContext là scoped, không dùng lại của request
        using var scope = scopeFactory.CreateScope();
        var writer  = scope.ServiceProvider.GetRequiredService<IChatRunWriter>();
        var sidecar = scope.ServiceProvider.GetRequiredService<ISidecarStreamClient>();

        // CancellationToken của run = app shutdown + user cancel (KHÔNG phải kết nối client)
        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(appStopping);
        RegisterCancellation(runId, runCts);

        try
        {
            await writer.MarkRunningAsync(runId);
            await writer.AppendAsync(runId, "run_started", new { });

            await foreach (var evt in sidecar.StreamAsync(runId, runCts.Token))
            {
                await writer.AppendAsync(runId, evt.Type, evt.Payload);
            }

            await writer.CompleteAsync(runId);       // lưu ChatMessage role=AI từ PartialOutput
        }
        catch (OperationCanceledException) when (runCts.IsCancellationRequested)
        {
            await writer.CancelAsync(runId);          // vẫn lưu phần đã sinh
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ChatRun] Run {RunId} thất bại", runId);
            await writer.FailAsync(runId, ex);
        }
        finally
        {
            UnregisterCancellation(runId);
        }
    }
}
```

### `ChatRunWriter.AppendAsync` — chống race trên `Seq`

Nhiều event ghi liên tiếp rất nhanh. Không dùng `MAX(Seq) + 1` (race + chậm).

```csharp
public async Task<long> AppendAsync(Guid runId, string type, object payload)
{
    // Tăng LastSeq nguyên tử ngay trong SQL
    var seq = await dbContext.ChatRuns
        .Where(r => r.Id == runId)
        .ExecuteUpdateAsync(s => s.SetProperty(r => r.LastSeq, r => r.LastSeq + 1));
    // ... đọc lại LastSeq hoặc dùng RETURNING tuỳ provider
}
```

> **Cân nhắc hiệu năng:** `text_delta` có thể phát sinh hàng trăm event/giây → ghi DB từng cái là
> quá tải. **Giải pháp: batching.** Gom `text_delta` trong bộ đệm, flush xuống DB mỗi
> **200ms hoặc 100 ký tự**, tuỳ điều kiện nào tới trước. Các event khác (`tool_start`,
> `plan_draft`, `error`...) ghi ngay lập tức vì chúng hiếm và quan trọng.
> Realtime vẫn publish từng chunk qua `IChatRunEventBus` để FE mượt — DB chỉ là nơi để tua lại.

> ⚠️ **Batching tạo cửa sổ mất dữ liệu ≤ 200ms nếu app chết đúng lúc.**
> Ba lớp bảo vệ bắt buộc — xem [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.5:
> flush trước mọi event quan trọng, cập nhật `PartialOutput` cùng nhịp flush, và flush khi
> `ApplicationStopping`.

---

## 8.5. Hub — tách "khởi động" khỏi "theo dõi"

`WebAPI/Hubs/ManagerChatHub.cs` viết lại:

```csharp
[Authorize]
public class ManagerChatHub(ISender sender, IChatRunEventBus bus) : Hub
{
    /// <summary>Khởi tạo run. Trả runId ngay, KHÔNG chờ AI chạy xong.</summary>
    public async Task<Guid> StartRun(Guid sessionId, string content)
    {
        var userId = ParseUserId();
        var token  = ExtractToken();
        return await sender.Send(new StartChatRunCommand(sessionId, content, userId, token));
    }

    /// <summary>Theo dõi run: tua từ afterSeq rồi nhận tiếp realtime.</summary>
    public async IAsyncEnumerable<ChatRunEventDto> SubscribeRun(
        Guid runId, long afterSeq,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var userId = ParseUserId();

        // 1. Replay phần đã lỡ (có kiểm tra quyền sở hữu bên trong query)
        var replay = await sender.Send(new GetChatRunEventsQuery(runId, afterSeq, userId), ct);
        long lastSeq = afterSeq;
        foreach (var e in replay)
        {
            lastSeq = e.Seq;
            yield return e;
        }

        // 2. Nếu run đã kết thúc trong lúc replay thì dừng luôn
        if (replay.RunIsTerminal) yield break;

        // 3. Nối vào luồng realtime, bỏ qua event đã replay
        await foreach (var e in bus.SubscribeAsync(runId, ct))
        {
            if (e.Seq <= lastSeq) continue;
            lastSeq = e.Seq;
            yield return e;
            if (IsTerminal(e.Type)) yield break;
        }
    }

    public Task CancelRun(Guid runId) =>
        sender.Send(new CancelChatRunCommand(runId, ParseUserId()));
}
```

**Điểm mấu chốt:** `ct` của `SubscribeRun` chỉ hủy **việc theo dõi**, không hủy **run**.
Client ngắt kết nối → run vẫn chạy tiếp, event vẫn ghi vào DB.

> **Khe hở cần xử lý:** giữa lúc replay xong và lúc subscribe bus có thể lọt event.
> **Cách xử lý:** subscribe bus **trước**, đệm event vào buffer, rồi mới replay từ DB,
> sau đó phát buffer và lọc theo `Seq <= lastSeq`. Viết test cho tình huống này.

---

## 8.6. Khôi phục khi người dùng quay lại

### API mới
```
GET /api/v1/manager-chat/sessions/{sessionId}/active-run
```
Trả về:
```json
{
  "runId": "…",
  "status": "Running",
  "lastSeq": 128,
  "startedAt": "2026-07-26T08:12:00Z",
  "userMessage": "Doanh thu tháng này?",
  "partialOutput": "Doanh thu tháng 7 hiện đạt "
}
```
Trả `null` nếu không có run nào đang chạy.

### Luồng phía FE khi mở lại session

```
1. GET  /sessions/{id}/history          → render tin nhắn đã hoàn tất
2. GET  /sessions/{id}/active-run       → có run đang chạy không?
   ├─ null   → hiển thị bình thường, sẵn sàng nhận input
   └─ có run → khôi phục UI ở trạng thái "AI đang trả lời"
        a. Render partialOutput làm bubble AI đang gõ
        b. hub.stream("SubscribeRun", runId, 0)   ← tua TOÀN BỘ event từ đầu
           (hoặc afterSeq = seq cuối đã render nếu còn giữ ở localStorage)
        c. Nhận tiếp realtime cho tới run_completed
```

**Lưu `{sessionId, runId, lastSeq}` vào `localStorage`** sau mỗi ~20 event để reload
trong cùng phiên trình duyệt tua nhanh hơn, nhưng **không phụ thuộc** vào nó — server luôn là
nguồn sự thật.

---

## 8.7. Dọn run mồ côi

Backend restart / crash giữa lúc run đang chạy → `ChatRun` kẹt ở `Running` vĩnh viễn.

`OrphanedRunCleaner` (BackgroundService):

1. **Lúc khởi động:** mọi `ChatRun` có `Status = Running` và `OwnerInstanceId` = instance hiện tại
   → chuyển sang `Orphaned`, append event `error` với code `run_orphaned`, lưu `PartialOutput`
   thành `ChatMessage` (đừng vứt phần đã sinh).
2. **Định kỳ mỗi 60s:** run có `HeartbeatAt` cũ hơn 2 phút → đánh dấu `Orphaned`.
3. `ChatRunExecutor` cập nhật `HeartbeatAt` mỗi 15s trong lúc chạy.

FE gặp `Orphaned` → hiện "Phiên trả lời bị gián đoạn" + nút **Thử lại**.

> ⚠️ **Nhưng nếu executor chết thì FE không nhận được event nào cả** — kể cả event `Orphaned`.
> Spinner sẽ quay vĩnh viễn. Cần **heartbeat event** mỗi 15s + watchdog 45s phía FE:
> xem [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.4.

> ⚠️ **Resume run sau restart phải revalidate registry tool.** Nếu tool đã bị gỡ trong lúc app
> dừng, run/plan cũ không còn chạy được — xem
> [17-STAGE-TOOL-LIFECYCLE.md](17-STAGE-TOOL-LIFECYCLE.md) mục 17.8.

---

## 8.8. Timeout & dọn dẹp

| Giới hạn | Giá trị | Nơi enforce |
|---|---|---|
| Thời gian tối đa 1 run | 5 phút | `runCts.CancelAfter(TimeSpan.FromMinutes(5))` |
| Số run đồng thời toàn hệ thống | 10 | `SemaphoreSlim` trong `ChatRunExecutor` |
| Số run đồng thời / user | 1 | Kiểm tra trong `StartChatRunCommandHandler` |
| Giữ `ChatRunEvent` | 7 ngày | Job dọn định kỳ (event chỉ để tua, không phải lịch sử chính) |

> ⚠️ **Giới hạn 1 run/user cần định nghĩa UX cho trường hợp 2 tab.**
> Tab B bấm gửi khi tab A đang chạy → coi là **steering** (Stage 9), không tạo run mới; cả hai tab
> thấy cùng stream. Chi tiết ở [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.4.

> ⚠️ **Run kết thúc phải dọn checkpoint ở sidecar**, nếu không state Python và `ChatRun.Status`
> của .NET sẽ lệch nhau. Ngoại lệ: plan ở `AwaitingApproval` giữ checkpoint tới 24h.
> Xem 18.3.

> **Quan trọng:** `ChatMessage` mới là lịch sử vĩnh viễn. `ChatRunEvent` là dữ liệu tạm phục vụ
> streaming/replay → xoá được sau vài ngày mà không mất nội dung hội thoại.

---

## 8.9. Sidecar phía Python

Sidecar cũng cần chịu được việc client .NET tạm ngắt. Vì run chạy nền ở .NET nên sidecar chỉ cần:

1. Nhận thêm `run_id` trong request để gắn vào log/trace.
2. Nhận được lệnh **huỷ tường minh** từ .NET. Cancel `runCts` ở .NET chỉ ngừng *đọc* stream —
   sidecar vẫn chạy tiếp LLM và tool, tốn token. Cần endpoint
   `POST /manager-chat/{run_id}/cancel`: xem
   [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.3.
3. Trả stream **JSON lines** thay vì text thô (thống nhất với Stage 6.4):
   ```
   {"type":"text_delta","content":"Doanh thu "}
   {"type":"tool_start","name":"get_sales_summary","callId":"c1"}
   {"type":"tool_end","callId":"c1","status":"ok","summary":"3 dòng dữ liệu"}
   {"type":"text_delta","content":"tháng 7 là 1.2 tỷ."}
   {"type":"done"}
   ```
3. **LangGraph checkpointer** để state của agent tồn tại qua các lần gọi — cần cho Stage 9 và 10:
   ```python
   from langgraph.checkpoint.postgres.aio import AsyncPostgresSaver
   # thread_id = run_id  → mỗi run là một luồng state riêng
   ```
   > **Quyết định cần chốt:** dùng `AsyncPostgresSaver` (dùng chung DB PostgreSQL của dự án,
   > cần cấp connection string cho sidecar) hay `MemorySaver` (đơn giản, mất khi restart).
   > Khuyến nghị: `MemorySaver` cho Stage 8, nâng lên Postgres khi làm Stage 10.

---

## 8.10. Ảnh hưởng dây chuyền — checklist đừng bỏ sót

- [ ] `StreamManagerChatMessageCommandHandler` cũ → thay bằng `StartChatRun` + executor.
- [ ] `ManagerChatHub.SendMessageStream` cũ → giữ tạm làm alias hoặc xoá hẳn (FE phải đổi cùng lúc).
- [ ] `ChatDrawer.vue` đổi từ `stream("SendMessageStream", ...)` sang
      `invoke("StartRun", ...)` + `stream("SubscribeRun", runId, afterSeq)`.
- [ ] `chat.api.ts` thêm `getActiveRun(sessionId)`.
- [ ] Test IDOR cho `SubscribeRun` và `GetChatRunEvents` (run của user khác → từ chối).

---

## Definition of Done — Stage 8

- [ ] Migration `ChatRun` + `ChatRunEvent` chạy được trên **cả** MySQL và PostgreSQL.
- [ ] Gửi tin nhắn → **đóng tab** → mở lại sau 20s → thấy AI vẫn đang gõ tiếp, nội dung liền mạch, không mất chữ.
- [ ] Đóng tab → đợi AI chạy xong → mở lại → thấy câu trả lời **đầy đủ** trong lịch sử.
- [ ] Bấm Dừng → run chuyển `Cancelled`, phần đã sinh được lưu, không có exception trong log.
- [ ] Kill process backend giữa lúc run chạy → khởi động lại → run thành `Orphaned`, FE hiện nút Thử lại.
- [ ] Mở **2 tab cùng lúc** trên cùng session → cả hai đều nhận stream đồng bộ.
- [ ] `text_delta` được batch, không tạo ra hàng nghìn INSERT mỗi câu trả lời.
- [ ] Test khe hở replay/subscribe: không mất và không lặp event.
