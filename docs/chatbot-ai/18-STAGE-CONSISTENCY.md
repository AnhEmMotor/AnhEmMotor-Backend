# Stage 18 — Nhất quán & Hoà giải trạng thái

> Nhóm B (9 ca) + C2–C7 + D1–D2 · Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 3–4 ngày
> Phụ thuộc cứng: **Stage 8, 9** — làm được ngay ở đợt 2.
>
> **Trạng thái (2026-07-28):** 18.1, 18.2, 18.4 (B6, B9), 18.5, 18.6 **đã xong**.
> Các mục còn lại cần Stage khác xong mới làm được, **bổ sung sau**:
> - **18.3 (phần dọn checkpoint mồ côi)** cần checkpointer bền (Postgres/Redis) — hiện sidecar
>   dùng `MemorySaver` trong bộ nhớ, không có gì để dọn. Cần Stage 12 (hoặc bất kỳ Stage nào
>   đổi checkpointer sang lưu bền) xong trước. *(Phần hoà giải huỷ 2 chiều của 18.3 đã xong ở
>   Stage 8/9.)*
> - **18.7** cần Stage 12 (Qdrant/RAG).
> - **18.8** cần Stage 17.4 (làm sạch lịch sử khi nạp lại) — hiện chưa có bước sanitize/nạp lại
>   lịch sử để gắn nhãn thời gian vào.
> - **18.9** cần Stage 14.6 (tóm tắt hội thoại) — tính năng tóm tắt chưa tồn tại nên chưa có gì
>   để áp quy tắc "không tóm tắt số liệu".
> - **18.10** cần Stage 11 (Reasoning Transparency) — panel "suy nghĩ" chưa tồn tại.
> - **18.11** cần Stage 11 — chưa có redaction ở đường ra FE nên chưa có gì để tách 2 đường dữ liệu.
> - **18.12** cần Stage 16 (Data Fidelity).
> - **18.13** cần Stage 22 (Multi-Agent) — chưa có sub-agent nào chạy nên chưa có trạng thái lồng
>   nào để hoà giải.

Hệ thống có **ba lớp giữ trạng thái độc lập**, và chúng sẽ lệch nhau:

```
┌──────────────┐   ┌──────────────────┐   ┌─────────────────────┐
│ FE (Vue)     │   │ .NET Run Engine  │   │ Sidecar (LangGraph) │
│ messages[]   │   │ ChatRun.Status   │   │ checkpointer state  │
│ lastSeq      │   │ ChatRunEvent     │   │ AgentState          │
│ plan version │   │ ChatPlan.Version │   │ messages in graph   │
└──────────────┘   └──────────────────┘   └─────────────────────┘
```

Stage này định nghĩa **ai là nguồn sự thật cho cái gì**, và cách hoà giải khi lệch.

---

## 18.1. Nguồn sự thật — chốt một lần

| Dữ liệu | Nguồn sự thật | Hai lớp kia làm gì |
|---|---|---|
| Lịch sử hội thoại | `ChatMessage` (.NET) | FE cache để hiển thị; sidecar nhận bản sạch hoá (Stage 17.4) |
| Trạng thái run | `ChatRun.Status` (.NET) | FE suy ra từ event; sidecar **không** quyết định |
| Nội dung đang stream | `ChatRunEvent` (.NET) | FE dựng lại từ `Seq`; sidecar chỉ phát ra |
| Plan | `ChatPlan` (.NET) | FE giữ `version` để phát hiện lệch; sidecar đọc lại mỗi bước |
| State suy luận của agent | checkpointer (sidecar) | .NET không cần biết nội dung, chỉ cần biết còn/hết |
| Permission | DB (.NET) | Sidecar chỉ nhận bản chụp theo run |

**Quy tắc bao trùm: .NET là nguồn sự thật cho mọi thứ người dùng nhìn thấy.**
Sidecar là bộ máy tính toán không có ký ức lâu dài — mất state của nó thì run làm lại được,
không mất dữ liệu người dùng.

---

## 18.2. Nhất quán nội tại trong một run (D1, D2)

**Ca lỗi:** AI trả lời *"còn 12 xe, giá 98 triệu"* — nhưng `12` đọc lúc 09:15:02 và `98 triệu`
đọc lúc 09:15:06 từ cache có TTL khác nhau. Không tool nào sai, nhưng **câu trả lời không đúng
tại bất kỳ thời điểm nào**.

### Snapshot theo run

```python
class RunSnapshot:
    """Ảnh chụp dữ liệu trong phạm vi một run. Mọi tool đọc qua đây."""

    def __init__(self, run_id: str):
        self.run_id = run_id
        self.as_of = None          # thời điểm đọc dữ liệu ĐẦU TIÊN của run
        self._cache: dict[str, dict] = {}

    async def get(self, tool_name: str, args: dict, fetcher) -> dict:
        key = f"{tool_name}:{json.dumps(args, sort_keys=True, default=str)}"
        if key in self._cache:
            return self._cache[key]

        result = await fetcher()
        if self.as_of is None:
            self.as_of = result.get("asOf")     # neo thời điểm của cả run
        self._cache[key] = result
        return result
```

### Ba quy tắc

1. **Cache theo run thay cho cache theo thời gian.** Bỏ TTL 60s ở Stage 14.5 cho tool đọc trong
   cùng run — dùng `RunSnapshot` (cache trong vòng đời run, hết run là xoá). Vừa nhất quán hơn
   vừa vẫn tiết kiệm y như cũ.
2. **`asOf` chung cho cả câu trả lời.** Envelope từng tool vẫn có `asOf` riêng, nhưng câu trả lời
   cuối dùng **`asOf` sớm nhất** và nói theo mốc đó: *"số liệu tính đến 09:15"*.
3. **Chênh lệch quá lớn thì cảnh báo.** Nếu `max(asOf) − min(asOf) > 60 giây` (run có gọi tool
   nhiều lần cách xa nhau), thêm `warnings: ["Dữ liệu được lấy ở các thời điểm cách nhau hơn 1 phút"]`
   và prompt buộc AI nhắc lại.

> **Lưu ý:** cache theo run **phải** key theo `run_id`, và run đã gắn với đúng một user
> (Stage 8.3) → tự động thoả mãn yêu cầu "cache key theo user" của Stage 14.5.

---

## 18.3. Hoà giải .NET ↔ Sidecar (B3, B4)

### B4 — User bấm Dừng nhưng sidecar vẫn stream

Hiện `ChatRunExecutor` cancel `runCts` → ngừng **đọc** stream, nhưng sidecar phía Python
**vẫn chạy tiếp** LLM và tool: tốn token, và nếu có tool ghi thì còn nguy hiểm.

**Cách sửa — huỷ hai chiều:**

```csharp
// Khi cancel run, gọi tường minh sang sidecar
private async Task CancelRunAsync(Guid runId)
{
    runCts.Cancel();
    try
    {
        await sidecarClient.PostAsync($"/manager-chat/{runId}/cancel", null,
                                      CancellationToken.None);
    }
    catch (Exception ex)
    {
        // Không để lỗi huỷ làm hỏng luồng chính
        logger.LogWarning(ex, "[ChatRun] Không huỷ được run {RunId} ở sidecar", runId);
    }
}
```

Phía sidecar giữ sổ đăng ký các run đang chạy:
```python
_active_runs: dict[str, asyncio.Event] = {}

@router.post("/manager-chat/{run_id}/cancel")
async def cancel_run(run_id: str, _: str = Depends(verify_internal_header)):
    if event := _active_runs.get(run_id):
        event.set()
    return {"cancelled": run_id in _active_runs}
```
Agent kiểm `event.is_set()` **ở mỗi ranh giới bước** (cùng chỗ với `absorb_steering` của Stage 9)
và trước khi gọi tool.

> **Không huỷ tool ghi đang chạy giữa chừng** — quy định này của Stage 9.5 vẫn giữ.
> Để nó chạy xong rồi mới dừng.

### B3 — Checkpointer còn state, .NET đã `Cancelled`

Khi run kết thúc ở bất kỳ trạng thái nào, .NET gọi sidecar dọn checkpoint:
```
DELETE /internal/agent/checkpoint/{runId}
```
Sidecar cũng chạy job dọn checkpoint mồ côi hằng ngày: `thread_id` không còn `ChatRun` tương ứng
→ xoá. Cần endpoint ngược lại để đối chiếu:
```
POST /internal/chat/runs/exists   { runIds: [...] }  →  { alive: [...] }
```

> **⚠️ Chưa làm được (2026-07-28):** `manager_agent.py` hiện dùng `MemorySaver()` — checkpoint
> chỉ sống trong bộ nhớ tiến trình, mất khi restart, không có gì để job "dọn hằng ngày" quét.
> Việc này chỉ có ý nghĩa sau khi đổi sang checkpointer bền (Postgres/Redis — dự kiến ở Stage 12
> hoặc khi làm hạ tầng đó). Cần làm để **phù hợp với DoD Stage 18**: "Job dọn checkpoint mồ côi
> chạy được, đối chiếu qua `/internal/chat/runs/exists`".

**Ngoại lệ:** plan ở `AwaitingApproval` (Stage 10) **phải giữ** checkpoint tới 24h — job dọn
phải loại trừ trạng thái này.

---

## 18.4. Hoà giải FE ↔ Server (B6, B8, B9)

### B8 — Run mồ côi nhưng FE vẫn quay spinner

Nguyên nhân: FE đang `SubscribeRun` nhưng không nhận event nào nữa (executor đã chết).

**Cách sửa — heartbeat event:** `ChatRunExecutor` phát event `run_heartbeat` mỗi 15 giây
(cùng nhịp cập nhật `HeartbeatAt` ở Stage 8.7). FE đặt watchdog 45 giây:

```ts
// Không nhận event nào trong 45s → chủ động kiểm tra lại
watchdog = setTimeout(async () => {
  const run = await ManagerChatApi.getActiveRun(sessionId);
  if (!run || run.runId !== currentRunId) {
    // Run đã kết thúc/mồ côi trong lúc mình không hay
    await reloadHistory();
    showInterruptedNotice();
  }
}, 45_000);
```

### B6 — Hai tab cùng session

Stage 8.8 giới hạn 1 run/user. Cần định nghĩa UX:

| Tình huống | Hành vi |
|---|---|
| Tab A đang chạy, tab B mở session đó | Tab B **thấy cùng stream** (đã có nhờ `SubscribeRun`) |
| Tab B bấm gửi khi tab A đang chạy | Coi như **steering** (Stage 9), không tạo run mới |
| Tab B bấm Dừng | Dừng run chung — hiện toast ở cả hai tab |

Tab B phải gọi `getActiveRun` **trước khi** cho gửi, để biết đang có run.

### B9 — Đổi tên session ở tab khác

Nhẹ nhưng gây bối rối. Cách xử lý rẻ nhất: khi cửa sổ lấy lại focus (`visibilitychange`),
FE tải lại danh sách session. Không cần realtime.

> **✅ Đã làm (2026-07-28):** B6 — `sendMessage` (`ChatDrawer.vue`) gọi `getActiveRun` trước khi
> tạo run mới nếu chưa tự thấy đang có stream, rồi resume + gửi dưới dạng steering. B9 —
> `visibilitychange` listener gọi `loadSessions()`. B8 (heartbeat/watchdog) đã có sẵn từ Stage 8.

---

## 18.5. Flush an toàn cho `text_delta` (B2)

> **Cập nhật (2026-07-29):** Stage 8.4 đã bỏ batching 200ms/100 ký tự — xem
> [08-STAGE-RUN-ENGINE.md](done/08-STAGE-RUN-ENGINE.md). Mỗi `text_delta` giờ flush
> `PartialOutput` **ngay khi tới**, không còn buffer nào chờ trong bộ nhớ giữa hai lần ghi.
> Cửa sổ mất dữ liệu ≤ 200ms nêu dưới đây **không còn tồn tại** cho `text_delta` — vẫn giữ
> nguyên bảng này cho các event khác vốn đã luôn ghi ngay (`tool_start`, `error`...), và cho
> `OrphanedRunCleaner` (8.7) vẫn cần để xử lý trường hợp instance chết hẳn giữa hai lần ghi
> (khác với "buffer chưa flush" — đó là do timeout heartbeat, không phải do batching).

| Lớp bảo vệ | Cách làm |
|---|---|
| Flush ngay mỗi `text_delta` | Không batching — xem 08-STAGE-RUN-ENGINE.md §8.4 |
| Flush trước mọi event quan trọng | Gặp `tool_start`, `plan_*`, `error`, `run_completed` → ghi ngay, không đổi |
| Flush khi app shutdown | Đăng ký `IHostApplicationLifetime.ApplicationStopping` → không còn buffer nào cần flush thêm |

---

## 18.6. Steering đang chờ phải nhìn thấy được (B5)

User gửi steering, `ChatMessage` đã lưu, nhưng agent chưa tới ranh giới bước → chưa `absorb`.
User thấy tin nhắn của mình mà AI như phớt lờ.

**Cách sửa:** phát event ngay khi nhận (Stage 9.2 đã định nghĩa `steering_received`), FE render
trạng thái rõ ràng:

| Event | Hiển thị |
|---|---|
| `steering_received` | `⏳ Đã ghi nhận, AI sẽ xử lý ở bước tiếp theo` |
| `steering_applied` | `✓ AI đã tiếp nhận` |
| Chờ > 20 giây chưa `applied` | `⏳ AI đang hoàn tất bước hiện tại...` + nút **Dừng và hỏi lại** |

Mốc 20 giây quan trọng: nếu agent đang gọi một tool chậm, user cần biết là hệ thống không treo.

---

## 18.7. Trích dẫn RAG phải đúng (C3)

AI nói *"theo chính sách bảo hành"* nhưng đoạn thật lấy từ tài liệu đổi trả.

**Cách sửa — trích dẫn có mã, verify được:**

Mỗi chunk trả về từ Qdrant kèm `citationId`:
```json
{
  "items": [
    {"citationId": "c1", "sourceFile": "warranty-policy.md",
     "heading": "Thời hạn bảo hành", "content": "..."}
  ]
}
```

Prompt bắt buộc:
```markdown
Khi dùng thông tin từ search_knowledge, PHẢI gắn mã trích dẫn ngay sau câu,
dạng [c1]. Không được nêu tên tài liệu nếu không có mã tương ứng.
```

**Output guard kiểm chứng** (mở rộng Stage 13.7):
```python
cited = set(re.findall(r"\[(c\d+)\]", answer))
available = {item["citationId"] for item in knowledge_results}
if invalid := cited - available:
    return GuardResult.rewrite(f"Mã trích dẫn không tồn tại: {invalid}. Chỉ dùng mã đã cung cấp.")
```

FE render `[c1]` thành chip bấm được, mở ra đoạn gốc — người dùng tự kiểm chứng được.

---

## 18.8. Số liệu cũ trong lịch sử hội thoại (C4)

**Ca lỗi:** lượt 1 AI nói *"SH 150i giá 98 triệu"*. 20 phút sau, lượt 5, user hỏi
*"cái xe lúc trước bao nhiêu tiền?"* → AI đọc lại từ lịch sử, nhưng giá đã đổi.

**Cách sửa — đóng dấu thời gian vào lịch sử:**

Khi nạp lại lịch sử (Stage 17.4), thêm ghi chú cho tin nhắn cũ hơn 15 phút:
```python
if age_minutes > 15:
    content = f"{item['message']}\n\n(Số liệu trong tin nhắn này tính đến {timestamp})"
```

Và trong system prompt:
```markdown
## Số liệu trong lịch sử hội thoại
Các con số trong tin nhắn trước đó là số liệu ở THỜI ĐIỂM ĐÓ, có thể đã thay đổi.
Khi người dùng hỏi lại về một con số cũ:
- Nếu tin nhắn cũ dưới 15 phút: dùng lại được, nhưng nêu rõ thời điểm.
- Nếu cũ hơn 15 phút: PHẢI tra cứu lại bằng tool, không đọc lại số cũ.
```

Mốc 15 phút là điểm cần hiệu chỉnh theo loại dữ liệu — giá và tồn kho đổi nhanh, danh mục thì không.

---

## 18.9. Tóm tắt hội thoại có kiểm chứng (C5)

Tóm tắt (Stage 14.6) làm mất chi tiết → lượt sau trả lời lệch.

| Quy tắc | Lý do |
|---|---|
| **Không tóm tắt số liệu** — chỉ tóm tắt chủ đề và quyết định | Số bị tóm tắt là số bị làm sai |
| Giữ nguyên văn 15 tin gần nhất | Ngữ cảnh gần luôn quan trọng nhất |
| Tóm tắt ghi rõ *"chi tiết số liệu đã lược bỏ, hãy tra cứu lại nếu cần"* | AI biết mà gọi tool thay vì đoán |
| Lưu `SummarizedUpToMessageId` vào `ChatSession` | Truy được tóm tắt phủ tới đâu |

**Migration** cho cột `ChatSession.SummarizedUpToMessageId` — **cả** MySQL và PostgreSQL:
```powershell
./add-migration.ps1 AddChatSessionSummaryPointer
```
| Tóm tắt là **bổ sung**, không thay thế `ChatMessage` | Lịch sử gốc vẫn nguyên vẹn trong DB |

---

## 18.10. `thinking` mâu thuẫn với câu trả lời (C6)

Panel suy nghĩ (Stage 11) nói *"cần kiểm tra thêm tồn kho"* nhưng câu trả lời lại khẳng định
chắc chắn — user đọc thấy mâu thuẫn, mất niềm tin.

**Nguyên nhân:** `<suy_nghi>` được sinh **trước** khi có kết quả tool; câu trả lời sinh sau.
Đây là hành vi bình thường, không phải bug — vấn đề là **cách trình bày**.

**Cách sửa:**
1. FE gắn nhãn rõ ràng trên panel: *"Diễn giải của AI trong quá trình xử lý"* — nói rõ đây là
   ghi chép quá trình, không phải kết luận.
2. Suy nghĩ hiển thị theo **thứ tự thời gian có mốc**, để thấy rõ tiến triển.
3. Output guard nhẹ: nếu `thinking` cuối cùng có từ khoá do dự (*"chưa rõ", "cần kiểm tra"*)
   mà câu trả lời **không** có bất kỳ diễn đạt độ chắc chắn nào → yêu cầu model bổ sung mức
   độ tin cậy. Cho phép tối đa 1 lần viết lại.

---

## 18.11. Quy ước cho giá trị đã che (C7)

Redaction (Stage 11.3) thay giá trị bằng `***`. Nếu **kết quả tool** đưa vào LLM cũng bị che,
AI sẽ diễn giải `***` như dữ liệu thật.

**Phân định dứt khoát — hai đường dữ liệu khác nhau:**

```
Kết quả tool
   ├─→ [đường 1] LLM        : dữ liệu ĐẦY ĐỦ, không redact
   └─→ [đường 2] ChatRunEvent → FE : ĐÃ redact (Stage 11)
```

Đây chính là thiết kế đã nêu ở Stage 11.5 (*"kết quả tool đầy đủ chỉ tồn tại trong bộ nhớ
sidecar, không ghi xuống `ChatRunEvent`"*) — mục này **làm rõ ràng bắt buộc**:

> `make_tool_preview()` chỉ được gọi khi phát event cho FE.
> **Tuyệt đối không** gọi nó trên dữ liệu đưa vào `ToolMessage` của LLM.

**Trường hợp buộc phải che cả với LLM** (ví dụ số CMND của khách — AI không cần biết):
dùng nhãn có nghĩa thay vì `***`:
```python
{"identityCard": "[đã ẩn vì lý do bảo mật]"}
```
Và prompt: *"Trường có giá trị `[đã ẩn...]` là thông tin bạn không được phép xem — hãy nói với
người dùng rằng thông tin đó cần tra cứu trực tiếp trên hệ thống."*

**Thêm test bắt buộc:** kết quả tool đưa vào LLM không được chứa chuỗi `***`.

---

## 18.12. Trộn dữ liệu nhiều kỳ (C2)

AI gọi `get_sales_summary` hai lần (tháng này, tháng trước) rồi trộn lẫn trong câu trả lời.

**Cách sửa:**
1. **Tool trả sẵn so sánh** — tham số `compare_with_previous` (Stage 14.2b) để chỉ có **một**
   nguồn số, không có chỗ trộn.
2. Nếu vẫn phải gọi nhiều lần: envelope bắt buộc có `periodLabel` (`"Tháng 7/2026"`), và prompt
   buộc AI ghi nhãn kỳ **cạnh mỗi con số**, không gộp thành một cụm.
3. Output guard: câu trả lời có ≥ 2 con số tiền tệ mà **không** có nhãn kỳ nào → yêu cầu viết lại.

---

## 18.13. Trạng thái sub-agent lồng trong một run (chờ Stage 22)

Stage 22 (Multi-Agent) cho agent cha sinh sub-agent tạm thời **trong cùng một `ChatRun`** — không
phải một lớp state thứ tư độc lập, nhưng vẫn có hai điểm cần hoà giải theo đúng khuôn khổ của Stage
này:

1. **`asOf` phải nhất quán giữa cha và con.** Sub-agent **không** được tạo `RunSnapshot` riêng —
   dùng chung instance của cha theo `run_id` (mục 18.2), nếu không sẽ tái diễn đúng lỗi *"còn 12 xe,
   giá 98 triệu"* đã nêu ở 18.2, nhưng lần này giữa hai tầng agent thay vì hai lần gọi tool.
2. **`Seq` của event sub-agent vẫn thuộc chuỗi `Seq` chung của run** (không đánh số riêng) — chỉ
   thêm field `subagentId` trong `Payload` để FE nhóm lại. Không cần cơ chế đối chiếu `Seq` mới.

---

## Definition of Done — Stage 18

- [ ] Bảng nguồn sự thật (18.1) được ghi vào `RULES.md`. **⚠️ Chưa** — `RULES.md` ở root
      backend hiện chỉ chứa quy chuẩn Git/code, không phải chỗ hợp cho quy tắc nghiệp vụ domain.
      Cần chốt vị trí ghi (ví dụ `docs/chatbot-ai/RULES.md` riêng) trước khi tích ô này.
- [x] `RunSnapshot` hoạt động; hai tool cùng run đọc cùng dữ liệu trả về **cùng một** giá trị.
- [x] Câu trả lời dùng `asOf` sớm nhất; lệch > 60s thì có `warnings` và AI nhắc lại.
- [ ] Bấm Dừng → sidecar **thực sự** dừng gọi LLM/tool (kiểm chứng bằng log token của provider).
      *(Cơ chế cancel 2 chiều đã có từ Stage 8/9; phần kiểm chứng bằng log token nhà cung cấp
      chưa làm.)*
- [ ] Run kết thúc → checkpoint được dọn; plan `AwaitingApproval` **không** bị dọn. **⚠️ Chưa** —
      xem ghi chú ở 18.3.
- [ ] Job dọn checkpoint mồ côi chạy được, đối chiếu qua `/internal/chat/runs/exists`. **⚠️ Chưa**
      — cần checkpointer bền trước (xem ghi chú ở 18.3).
- [ ] Kill executor giữa run → FE phát hiện trong 45s và hiện thông báo gián đoạn (không quay mãi).
      *(Watchdog 45s đã có từ Stage 8; chưa kiểm thử end-to-end kill thật.)*
- [x] Hai tab: tab B thấy cùng stream; bấm gửi ở tab B thành steering, không tạo run thứ hai.
- [x] Flush buffer trước mọi event quan trọng và khi app shutdown.
- [x] Steering chờ > 20s → FE hiện trạng thái + nút Dừng và hỏi lại.
- [ ] Trích dẫn RAG dùng mã `[c1]`; mã không tồn tại → bị guard chặn và viết lại. **Chờ Stage 12.**
- [ ] FE render trích dẫn thành chip bấm mở được đoạn gốc. **Chờ Stage 12.**
- [ ] Hỏi lại số liệu cũ hơn 15 phút → AI **tra cứu lại**, không đọc số cũ. **Chờ Stage 17.4.**
- [ ] Tóm tắt hội thoại **không** chứa số liệu; `SummarizedUpToMessageId` được lưu. **Chờ Stage 14.6.**
- [ ] Panel suy nghĩ có nhãn "diễn giải trong quá trình xử lý". **Chờ Stage 11.**
- [ ] **Test: dữ liệu vào LLM không chứa `***`** — redaction chỉ áp cho đường ra FE. **Chờ Stage 11.**
- [ ] Câu trả lời so sánh nhiều kỳ luôn có nhãn kỳ cạnh từng con số. **Chờ Stage 16.**
- [ ] Sub-agent dùng chung `RunSnapshot` và chuỗi `Seq` của run cha, không tạo trạng thái riêng.
      **Chờ Stage 22.**

### Test

`AISidecar/tests/test_consistency.py`:
- [x] `RunSnapshot.get` gọi 2 lần cùng `(tool, args)` → `fetcher` chỉ chạy **1 lần**.
- [x] `as_of` neo theo lần đọc **đầu tiên**, không đổi ở các lần sau.
- [x] Lệch `asOf` > 60s → sinh `warnings`.
- [ ] **Dữ liệu vào LLM không chứa `***`** — redaction chỉ áp cho đường ra FE (18.11). **Chờ Stage 11.**
- [ ] Mã trích dẫn `[c9]` không có trong kết quả → output guard yêu cầu viết lại. **Chờ Stage 12.**
- [ ] Tin nhắn cũ > 15 phút được gắn nhãn thời điểm khi nạp lại lịch sử. **Chờ Stage 17.4.**
- [ ] Sub-agent và cha đọc cùng một `RunSnapshot` (cùng `asOf`) trong cùng một run. **Chờ Stage 22.**

`UnitTests/ManagerChatRun.cs`:
- [ ] Cancel run → gọi endpoint `/manager-chat/{runId}/cancel` của sidecar (mock, verify).
- [ ] Run kết thúc → gọi `DELETE /internal/agent/checkpoint/{runId}`.
- [ ] Run `AwaitingApproval` → **không** bị job dọn checkpoint đụng vào.
- [ ] Replay + subscribe: không mất event, không lặp `Seq` (test khe hở ở 8.5).
- [ ] Flush buffer trước `tool_start` và khi `ApplicationStopping`.
