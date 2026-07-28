# Stage 16 — Độ chính xác dữ liệu: chống tool trả số sai lệch

> Yêu cầu bổ sung · Ưu tiên: 🔴 Cao · Ước lượng: 3–4 ngày · Phụ thuộc: **Stage 3, 13**
> Mục tiêu: tool gọi **đúng** rồi thì số trả về cũng phải **đúng** — khớp với báo cáo trên UI,
> không lệch kỳ, không thiếu bộ lọc, không âm thầm cắt bớt.

---

## 16.1. Khác gì với Stage 13?

| | Stage 13 — Guardrails | **Stage 16 — Data Fidelity** |
|---|---|---|
| Câu hỏi | AI có gọi **đúng tool** không? | Tool có trả **đúng số** không? |
| Lỗi điển hình | Hỏi tồn kho → gọi `get_sales_summary` | Gọi đúng `get_sales_summary` nhưng số lệch 12% so với báo cáo |
| Ai phát hiện | Eval tự động | **Người dùng nghiệp vụ** — và họ mất niềm tin ngay lập tức |
| Hậu quả | AI trả lời lạc đề, dễ nhận ra | AI trả lời **tự tin và sai** — nguy hiểm hơn nhiều |

> Một chatbot trả lời sai lạc đề thì người dùng bỏ qua. Một chatbot đưa ra con số doanh thu sai
> nhưng nghe hợp lý thì người ta ra quyết định dựa trên nó. **Đây là rủi ro nghiêm trọng nhất
> của cả dự án.**

---

## 16.2. Chín nguồn sai lệch

Các mục đánh dấu ✅ là **đã xác minh trong codebase này**, không phải rủi ro lý thuyết.

### 1. Bỏ qua soft-delete filter ✅

`Infrastructure/DBContexts/ApplicationDBContext.cs` áp global query filter theo
`BaseEntity.DeletedAt == null` cho mọi entity. Nhưng có **115 chỗ** dùng `IgnoreQueryFilters()`
hoặc `All<T>()` để cố tình vượt filter:

```bash
grep -rn "IgnoreQueryFilters\|\.All<" Application Infrastructure --include="*.cs" | grep -v obj/ | wc -l
# → 115
```

**Rủi ro:** tool chat tái sử dụng nhầm một query thuộc nhóm này → đếm cả bản ghi đã xoá mềm
→ số cao hơn báo cáo chính thức mà không ai biết vì sao.

**Bắt buộc:** mọi query của tool chat **không được** dùng `IgnoreQueryFilters()` / `All<T>()`.

Test chặn tự động — `UnitTests/ChatToolsGuard.cs`, cùng kiểu với `SidecarConfigGuard.cs` ở Stage 1.6.4:

```csharp
[Fact(DisplayName = "GUARD_10 - Tool chat không được vượt qua soft-delete filter")]
public void ChatTools_KhongDung_IgnoreQueryFilters()
{
    var dir = Path.Combine(RepoRoot(), "Application", "Features", "ChatTools");
    var offenders = Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)
        .Where(f =>
        {
            var content = File.ReadAllText(f);
            return content.Contains("IgnoreQueryFilters") || content.Contains(".All<");
        })
        .Select(f => Path.GetRelativePath(RepoRoot(), f))
        .ToList();

    offenders.Should().BeEmpty(
        "tool chat phải tôn trọng global query filter DeletedAt == null, xem mục 16.2");
}
```

### 2. Múi giờ ✅

Toàn bộ dự án dùng `DateTimeOffset.UtcNow` và **không có bất kỳ xử lý timezone nào**:

```bash
grep -rni "timezone|SE Asia|GMT+7|AddHours(7)" Application Infrastructure WebAPI --include="*.cs"
# → không có kết quả
```

**Rủi ro cụ thể:** người dùng ở GMT+7 hỏi *"doanh thu hôm nay"* lúc 06:00 sáng ngày 26/07.
UTC lúc đó là 23:00 ngày 25/07 → nếu tool lấy "hôm nay theo UTC" thì trả về **ngày 25/07**.
Sai nguyên một ngày, và chỉ sai trong khung 00:00–07:00 → rất khó phát hiện, rất dễ đổ lỗi nhầm.

**Bắt buộc:**
- Sidecar **không** tự tính "hôm nay". Backend trả `serverDate` (theo GMT+7) trong context (Stage 2).
- Tool nhận ngày dạng `YYYY-MM-DD` hiểu theo **giờ Việt Nam**, backend tự quy đổi sang UTC khi query.
- Response envelope ghi rõ `"timezone": "Asia/Ho_Chi_Minh"`.

### 3. Cắt bớt im lặng (silent truncation)

Tool trả 10 bản ghi trong tổng số 487. AI đọc và kết luận *"có 10 sản phẩm sắp hết hàng"*.

**Bắt buộc:** mọi tool danh sách phải trả `totalCount` và `truncated`, và prompt phải hướng dẫn
AI diễn đạt đúng: *"10 trong tổng số 487 sản phẩm sắp hết hàng"*.

### 4. Định nghĩa nghiệp vụ không thống nhất

`Application/Features/Statistical/Queries/` có nhiều nguồn cho cùng khái niệm "doanh thu":
`GetDailyRevenue`, `GetMonthlyRevenueProfit`, `GetAdminRevenueAnalysis`, `GetPnlReport`,
`GetRevenueByCategory`, và `SalesReports/GetSalesReport`.

**Rủi ro:** ba query này rất có thể định nghĩa doanh thu khác nhau (trước/sau chiết khấu,
có/không VAT, tính khi đặt hàng hay khi giao thành công, có/không đơn huỷ). Tool chọn nguồn
khác với báo cáo mà giám đốc đang xem → hai con số khác nhau cho cùng câu hỏi.

**Bắt buộc:** lập từ điển nghiệp vụ (mục 16.4) **trước khi** viết tool tài chính.

### 5. Trạng thái đơn hàng bị bỏ sót

`Outputs/Queries/` có `GetOrderCancellableStatuses`, `GetOrderLockedStatuses`,
`GetOrderStatusMap`, và `Order` có khái niệm `DraftOrderManagement`.

**Rủi ro:** tool đếm đơn hàng gộp cả đơn nháp và đơn đã huỷ → doanh số phồng lên.

**Bắt buộc:** mọi tool liên quan đơn hàng khai báo tường minh danh sách trạng thái được tính,
và ghi vào envelope `filtersApplied`.

### 5b. Trộn dữ liệu nhiều kỳ trong cùng câu trả lời

AI gọi `get_sales_summary` hai lần (tháng này, tháng trước) rồi trộn số của hai kỳ.

**Bắt buộc:** envelope có `periodLabel`; ưu tiên tool trả sẵn so sánh qua `compare_with_previous`
để chỉ có một nguồn số. Chi tiết ở
[18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.12.

### 6. Nhân bản dòng do JOIN

Query doanh thu JOIN sang bảng chi tiết đơn hàng mà không `DISTINCT`/group đúng → một đơn 3 mặt
hàng bị đếm 3 lần.

**Bắt buộc:** parity test (mục 16.6) so với endpoint UI sẽ bắt được lỗi này.

### 7. Dữ liệu cũ từ cache và Qdrant

- Cache kết quả tool (Stage 14.5) TTL 60s → tồn kho có thể lệch.
- Payload Qdrant (Stage 12) cập nhật trễ → giá/tồn kho cũ.

**Bắt buộc:** đã quy định ở Stage 12.5 — giá và tồn kho **luôn lấy lại từ SQL**.
Envelope ghi `asOf` để AI nói được *"số liệu tính đến 09:15 hôm nay"*.

> ⚠️ **TTL khác nhau giữa các tool còn tạo ra sai lệch nội tại trong cùng một câu trả lời.**
> Thay TTL bằng cache theo phạm vi run (`RunSnapshot`) để mọi tool trong một run đọc cùng ảnh chụp
> dữ liệu — xem [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.2.

### 8. DTO trôi (drift)

Tool tái sử dụng DTO của UI. Sáu tháng sau ai đó đổi tên field `TotalRevenue` → `NetRevenue`.
Tool không lỗi biên dịch (deserialize sang `null`) → AI báo doanh thu **0 đồng**.

**Bắt buộc:** contract test snapshot (mục 16.7).

### 9. `null` bị hiểu thành `0`

"Không có dữ liệu" và "bằng không" là hai chuyện khác nhau. Chi nhánh chưa nhập liệu ≠ chi nhánh
doanh thu 0đ.

**Bắt buộc:** envelope phân biệt rõ; prompt cấm AI tự quy `null` thành `0`.

---

## 16.3. Nguyên tắc nền: tái sử dụng, không viết lại

**Quy tắc số 1 của Stage 15.5 bước 1 nay thành bắt buộc tuyệt đối:**

> Handler của tool chat **gọi lại** query/handler mà UI đang dùng, chỉ đổi hình dạng DTO.
> **Không** viết truy vấn LINQ/SQL mới cho tool chat.

```csharp
// ✅ ĐÚNG — tái sử dụng, sai lệch là bất khả thi về mặt cấu trúc
public async Task<ChatSalesSummaryDto> Handle(GetSalesSummaryForChatQuery request, CancellationToken ct)
{
    var report = await sender.Send(new GetDailyRevenueQuery(request.From, request.To), ct);
    return ChatSalesSummaryDto.From(report);   // chỉ reshape
}

// ❌ SAI — viết truy vấn riêng, chắc chắn sẽ trôi khỏi báo cáo chính sau vài tháng
var total = await dbContext.Outputs
    .Where(o => o.CreatedAt >= request.From && o.CreatedAt <= request.To)
    .SumAsync(o => o.TotalAmount, ct);
```

Lợi ích: khi nghiệp vụ đổi cách tính doanh thu, UI và chatbot đổi **cùng lúc**, tự động.

**Ngoại lệ được phép:** khi query gốc trả về quá nặng (kèm ảnh, phân trang phức tạp) thì viết
projection riêng — nhưng **phải** có parity test (16.6) chứng minh cùng kết quả.

---

## 16.4. Từ điển nghiệp vụ — làm trước khi code tool tài chính

Tạo `docs/chatbot-ai/GLOSSARY.md`, chốt cùng người phụ trách nghiệp vụ:

```markdown
| Khái niệm | Định nghĩa chốt | Nguồn chuẩn | Loại trừ |
|---|---|---|---|
| Doanh thu | Tổng tiền hàng sau chiết khấu, chưa gồm phí ship | `Statistical/GetDailyRevenue` | Đơn huỷ, đơn nháp, đơn hoàn |
| Số đơn hàng | Đơn có trạng thái ∈ {…} | `Outputs/GetOutputsList` | Nháp, huỷ |
| Lợi nhuận | Doanh thu − giá vốn − chi phí | `Statistical/GetPnlReport` | — |
| Tồn kho | Số lượng khả dụng tại kho | `InventoryOnHand` | Hàng đang giữ cho đơn chưa giao |
| Khách hàng mới | Có đơn đầu tiên trong kỳ | `Customer/GetCustomerProfile360` | — |
| "Tháng này" | Từ ngày 1 đến hôm nay, giờ Việt Nam | — | — |
```

**Cách dùng:**
1. Mỗi tool ghi rõ mình dùng định nghĩa nào.
2. Trích đoạn liên quan vào system prompt để AI diễn đạt nhất quán.
3. Ô "Loại trừ" đưa thẳng vào `filtersApplied` của envelope.

> Đây là hạng mục **rẻ nhất và có tác động lớn nhất** của cả Stage. Phần lớn tranh cãi
> "số của AI sai" thực ra là hai bên đang dùng hai định nghĩa khác nhau.

---

## 16.5. Envelope bắt buộc cho mọi tool

Không tool nào được trả về dữ liệu trần. Mọi kết quả bọc trong envelope:

```csharp
public record ChatToolEnvelope<T>(
    T Data,
    int TotalCount,
    bool Truncated,
    DateTimeOffset AsOf,                          // thời điểm tính số liệu
    string Timezone,                              // luôn "Asia/Ho_Chi_Minh"
    string Source,                                // "Statistical/GetDailyRevenue"
    IReadOnlyList<string> FiltersApplied,         // ["Loại trừ đơn huỷ", "Loại trừ đơn nháp"]
    string? Definition,                           // trích từ GLOSSARY.md
    string? Currency = "VND",
    IReadOnlyList<string>? Warnings = null        // ["Chi nhánh B chưa có dữ liệu"]
);
```

Ví dụ thực tế:
```json
{
  "data": { "totalRevenue": 1240000000, "orderCount": 312 },
  "totalCount": 312,
  "truncated": false,
  "asOf": "2026-07-26T09:15:00+07:00",
  "timezone": "Asia/Ho_Chi_Minh",
  "source": "Statistical/GetDailyRevenue",
  "filtersApplied": ["Loại trừ đơn huỷ", "Loại trừ đơn nháp", "Sau chiết khấu, chưa gồm phí ship"],
  "definition": "Doanh thu = tổng tiền hàng sau chiết khấu, chưa gồm phí vận chuyển",
  "currency": "VND"
}
```

### Prompt phải bắt AI dùng envelope

Bổ sung vào `app/prompts/system_manager_chat.md`:

```markdown
## Quy tắc trình bày số liệu — BẮT BUỘC

Khi nêu bất kỳ con số nào lấy từ tool:
1. Nêu rõ PHẠM VI THỜI GIAN và các bộ lọc đã áp dụng (trường `filtersApplied`).
2. Nếu `truncated = true`, phải nói rõ đây chỉ là một phần:
   "10 trong tổng số 487 sản phẩm".
3. Nếu có `warnings`, phải nhắc lại cho người dùng.
4. Nếu số liệu cũ hơn 15 phút, nêu thời điểm: "tính đến 09:15".
5. KHÔNG được tự cộng/trừ/nhân/chia các con số từ nhiều tool khác nhau.
   Nếu cần so sánh hay tính tỷ lệ, hãy gọi tool có sẵn chức năng đó.
6. Nếu một giá trị là null, nói "chưa có dữ liệu" — KHÔNG nói "bằng 0".
7. Nếu tool trả lỗi hoặc bị từ chối quyền, KHÔNG nêu bất kỳ con số nào.
```

Điểm 5 quan trọng: **LLM tính toán số học rất hay sai**, đặc biệt với số lớn kiểu tiền Việt Nam.
Nếu cần "tăng bao nhiêu phần trăm so với tháng trước" thì tool phải trả sẵn (Stage 14.2b đã
khuyến nghị tham số `compare_with_previous`), không để AI tự chia.

---

## 16.6. Parity test — vũ khí chính

Với cùng tham số, tool chat và endpoint UI **phải cho cùng kết quả**.

`IntegrationTests/ChatToolParity.cs` (file phẳng theo quy ước repo — xem Stage 1.6.1):

```csharp
[Theory]
[InlineData("2026-07-01", "2026-07-26")]
[InlineData("2026-06-01", "2026-06-30")]
[InlineData("2026-01-01", "2026-12-31")]   // cả năm
[InlineData("2026-07-26", "2026-07-26")]   // 1 ngày
public async Task SalesSummaryTool_KhopVoiBaoCaoUI(string from, string to)
{
    var uiResult   = await sender.Send(new GetDailyRevenueQuery(Parse(from), Parse(to)));
    var toolResult = await sender.Send(new GetSalesSummaryForChatQuery(Parse(from), Parse(to)));

    toolResult.Data.TotalRevenue.Should().Be(uiResult.TotalRevenue);
    toolResult.Data.OrderCount.Should().Be(uiResult.OrderCount);
}
```

### Các trường hợp biên bắt buộc test

| Trường hợp | Bắt lỗi gì |
|---|---|
| Khoảng thời gian rỗng (không có đơn) | `null` vs `0` |
| Đúng 1 ngày | Lỗi biên `>=` / `>` |
| Qua ranh giới tháng / năm | Lỗi cộng dồn |
| **Truy vấn lúc 00:00–07:00 giờ VN** | **Lỗi múi giờ** — bắt buộc có |
| Có bản ghi đã xoá mềm trong kỳ | Lọt `IgnoreQueryFilters` |
| Có đơn huỷ / đơn nháp trong kỳ | Thiếu bộ lọc trạng thái |
| Đơn nhiều mặt hàng | Nhân bản dòng do JOIN |
| Dữ liệu vượt `limit` | Truncation |

**Mỗi tool trả số liệu tài chính hoặc tổng hợp đều phải có parity test.**
Tool danh sách đơn giản (liệt kê thương hiệu) thì không bắt buộc.

---

## 16.7. Contract test — chống DTO trôi

File: `UnitTests/ChatToolContracts.cs` (file phẳng — xem Stage 1.6.1).
Snapshot cấu trúc DTO của tool. Đổi field mà quên cập nhật tool → test đỏ ngay.

> **Cần thêm package snapshot** vào `UnitTests.csproj` — dự án hiện **chưa có**.
> Khuyến nghị `Verify.Xunit` (hoặc tự so sánh với file JSON đã commit nếu không muốn thêm
> dependency). Ghi rõ lựa chọn vào PR của Stage này.

```csharp
[Fact(DisplayName = "CONTRACT_01 - DTO doanh thu cho chat giữ nguyên hợp đồng")]
public void ChatSalesSummaryDto_GiuNguyenHopDong()
{
    var schema = JsonSchemaGenerator.Generate<ChatToolEnvelope<ChatSalesSummaryDto>>();
    schema.Should().MatchSnapshot("chat-sales-summary.schema.json");
}
```

Và ở sidecar, validate response trước khi đưa vào LLM:

```python
class SalesSummaryResult(BaseModel):
    model_config = ConfigDict(extra="forbid")   # field lạ → lỗi ngay, không im lặng

    total_revenue: int
    order_count: int
    as_of: datetime
    filters_applied: list[str]
```

`extra="forbid"` quan trọng: nếu backend đổi tên field, sidecar **báo lỗi rõ ràng** thay vì
đưa `None` cho LLM và để nó tự bịa.

---

## 16.8. Triển khai tool mới theo kiểu canary

Tool mới không bật thẳng cho tất cả người dùng.

| Giai đoạn | Thời gian | Ai dùng | Việc cần làm |
|---|---|---|---|
| **Shadow** | 3–5 ngày | Không ai | Tool chạy nền song song với báo cáo UI, chỉ ghi log so sánh; lệch > 0.1% → cảnh báo |
| **Canary** | 1 tuần | 2–3 người dùng nội bộ | Thu thập phản hồi "số này có đúng không" |
| **Full** | — | Tất cả | Sau khi shadow không còn cảnh báo |

Cờ bật/tắt theo tool trong `appsettings.json`:
```jsonc
"AISetup": {
    "ToolFlags": {
        "get_sales_summary": "full",
        "get_pnl_report": "canary",
        "get_payroll_summary": "off"
    }
}
```
`build_tools` (Stage 13.2) đọc cờ này — tool `off` không vào registry, tool `canary` chỉ cấp cho
user trong danh sách thử nghiệm.

> Cờ này cũng là **nút tắt khẩn cấp**: phát hiện tool trả sai trong production → đặt `off`,
> không cần deploy lại.

---

## 16.9. Nút phản hồi "Số này sai"

Cách phát hiện sai lệch hiệu quả nhất là để người dùng nói.

Trong `ChatDrawer.vue`, mỗi tin nhắn AI có chứa số liệu → thêm nút nhỏ 👎 **Số liệu chưa đúng**.
Bấm vào ghi lại: `runId`, các tool đã gọi, tham số, kết quả tool, câu trả lời — vào bảng
`ChatFeedback` hoặc log riêng để đội phát triển đối chiếu.

Vì Stage 8 đã lưu toàn bộ `ChatRunEvent`, việc dựng lại "AI đã lấy số từ đâu" là tra một truy vấn.

---

## 16.10. Giám sát trong production

| Cảnh báo | Ngưỡng | Nghĩa là |
|---|---|---|
| Tool trả `totalCount = 0` bất thường | > 20% lượt gọi trong 1 giờ | Bộ lọc sai hoặc dữ liệu chưa nhập |
| Tool trả `truncated = true` | > 50% lượt gọi | `limit` quá thấp, AI đang thấy dữ liệu thiếu |
| Shadow diff | > 0.1% | Tool lệch báo cáo UI |
| Phản hồi 👎 số liệu sai | > 3 lượt/tuần cho cùng 1 tool | Điều tra ngay |
| Tool trả `null` cho field bắt buộc | bất kỳ | DTO đã trôi |

---

## 16.11. Checklist thêm tool mới — mở rộng từ Stage 15.5

Khuôn mẫu 5 bước ở [15-STAGE-TOOL-CATALOG.md](15-STAGE-TOOL-CATALOG.md) nay thành **9 bước**:

1. Query .NET — **tái sử dụng** handler của UI, không viết truy vấn mới
2. Endpoint + `[RequirePermission]`
3. Khai báo `ToolSpec` (permission, `is_write`, module)
4. Mô tả theo template `DÙNG KHI / KHÔNG DÙNG KHI` (Stage 13.9)
5. Summarizer + nhãn tiếng Việt (Stage 11.3)
6. **Envelope đầy đủ** — `asOf`, `filtersApplied`, `totalCount`, `truncated`, `source`, `definition`
7. **Parity test** so với endpoint UI, gồm đủ trường hợp biên ở 16.6
8. **Contract test** snapshot DTO + `extra="forbid"` phía sidecar
9. **Đặt cờ `shadow`**, chạy 3–5 ngày rồi mới nâng lên `canary` → `full`

**Không đủ 9 bước thì không merge.** Ghi quy định này vào `RULES.md`.

---

## Definition of Done — Stage 16

- [ ] `GLOSSARY.md` đã lập và được người phụ trách nghiệp vụ xác nhận.
- [ ] Không tool chat nào dùng `IgnoreQueryFilters()` / `All<T>()` — có test tự động chặn.
- [ ] Backend trả `serverDate` theo GMT+7 trong context; sidecar **không** tự tính "hôm nay".
- [ ] Test truy vấn "hôm nay" lúc 00:00–07:00 giờ VN cho kết quả đúng ngày.
- [ ] Mọi tool trả envelope đầy đủ; không tool nào trả dữ liệu trần.
- [ ] Parity test cho **mọi** tool tài chính/tổng hợp, phủ đủ 8 trường hợp biên ở 16.6.
- [ ] Contract test snapshot chạy trong CI; đổi DTO mà quên tool → test đỏ.
- [ ] Sidecar validate response với `extra="forbid"`.
- [ ] Cờ `ToolFlags` hoạt động; đặt `off` là tool biến mất khỏi registry ngay, không cần deploy.
- [ ] AI diễn đạt đúng khi `truncated = true` ("10 trong tổng số 487").
- [ ] AI nói "chưa có dữ liệu" thay vì "0" khi giá trị `null`.
- [ ] AI **không tự tính toán** giữa các tool — có eval case kiểm chứng.
- [ ] Nút phản hồi "Số liệu chưa đúng" hoạt động, truy ngược được về `ChatRunEvent`.
- [ ] Shadow mode chạy được và ghi log diff.
