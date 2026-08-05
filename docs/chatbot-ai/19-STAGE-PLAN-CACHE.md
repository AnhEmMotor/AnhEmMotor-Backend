# Stage 19 — Cache Plan: giảm số lần suy nghĩ

> Yêu cầu bổ sung · Ưu tiên: 🟠 Trung bình · Ước lượng: 3–4 ngày
> Phụ thuộc: **Stage 10 (Plan Mode), 12 (Qdrant), 17 (fingerprint)**

Mục tiêu: câu hỏi đã từng được lập kế hoạch thì **không lập lại từ đầu** — tái dùng plan cũ,
tiết kiệm chi phí và thời gian suy nghĩ.

---

## 19.1. Vì sao đáng làm

Lập plan là bước **đắt nhất** của một run: model phải đọc mô tả nhiều tool, suy luận thứ tự,
sinh 4–8 bước — thường 2.000–4.000 token đầu ra và 3–6 giây.

Nhưng công việc quản lý cửa hàng có tính **lặp lại rất cao**:

| Câu hỏi lặp | Tần suất thực tế |
|---|---|
| "Báo cáo tồn kho tuần này" | Mỗi tuần, nhiều người |
| "Doanh thu tháng này so với tháng trước" | Mỗi ngày |
| "Sản phẩm nào sắp hết hàng" | Mỗi ngày |
| "Tình hình đơn hàng chưa giao" | Nhiều lần/ngày |

Cùng một *ý định*, chỉ khác *tham số* (kỳ báo cáo, danh mục). **Cấu trúc plan giống nhau hoàn toàn.**

### Lợi ích ước tính

| | Không cache | Có cache (hit) |
|---|---|---|
| Token đầu ra cho bước lập plan | 2.000–4.000 | ~0 |
| Thời gian lập plan | 3–6s | < 0.5s |
| Số vòng agent | +1 đến +2 | +0 |
| Tính nhất quán giữa các lần hỏi | Plan khác nhau mỗi lần | **Giống nhau** |

> Mục cuối bảng có giá trị ngoài dự tính: người dùng hỏi cùng câu ở hai thời điểm hiện nhận
> **cùng một kế hoạch** → số liệu so sánh được với nhau. Không cache thì AI có thể chọn 2 cách
> tính khác nhau cho cùng câu hỏi.

---

## 19.2. Data model

`Domain/Entities/ChatPlanTemplate.cs`
```csharp
[Table("ChatPlanTemplate")]
public class ChatPlanTemplate : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Câu hỏi đại diện đã sinh ra template này (để đối chiếu và debug).</summary>
    [Required]
    [Column("CanonicalQuestion", TypeName = "nvarchar(500)")]
    public string CanonicalQuestion { get; set; } = string.Empty;

    /// <summary>Hash chuẩn hoá của ý định — khoá tra cứu chính xác.</summary>
    [Required]
    [Column("IntentHash", TypeName = "nvarchar(64)")]
    public string IntentHash { get; set; } = string.Empty;

    /// <summary>Các bước đã tham số hoá (slot thay cho giá trị cụ thể), JSON.</summary>
    [Required]
    [Column("StepsTemplate", TypeName = "nvarchar(max)")]
    public string StepsTemplate { get; set; } = "[]";

    /// <summary>Danh sách slot cần điền, JSON. Ví dụ: from_date, to_date, category.</summary>
    [Required]
    [Column("Slots", TypeName = "nvarchar(max)")]
    public string Slots { get; set; } = "[]";

    /// <summary>Tool mà template cần — dùng để vô hiệu khi tool bị gỡ (Stage 17).</summary>
    [Required]
    [Column("RequiredTools", TypeName = "nvarchar(max)")]
    public string RequiredTools { get; set; } = "[]";

    /// <summary>Permission tối thiểu để dùng template này.</summary>
    [Required]
    [Column("RequiredPermissions", TypeName = "nvarchar(max)")]
    public string RequiredPermissions { get; set; } = "[]";

    /// <summary>Fingerprint registry lúc template được tạo (Stage 17.2).</summary>
    [Column("ToolRegistryFingerprint", TypeName = "nvarchar(32)")]
    public string? ToolRegistryFingerprint { get; set; }

    // --- Thống kê để xếp hạng và dọn dẹp ---
    public int UseCount { get; set; }
    public int SuccessCount { get; set; }
    public int UserEditCount { get; set; }      // user sửa bao nhiêu lần → template chưa tốt
    public int RejectCount { get; set; }        // user huỷ bao nhiêu lần
    public DateTimeOffset? LastUsedAt { get; set; }

    [Required]
    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = "active";   // active | stale | disabled
}
```

Migration cho **cả** MySQL và PostgreSQL:
```powershell
./add-migration.ps1 AddChatPlanTemplate
```

---

## 19.3. Khoá cache — không dùng chuỗi thô

Cache theo chuỗi câu hỏi nguyên văn gần như không bao giờ hit:
*"Doanh thu tháng này"* ≠ *"doanh thu tháng này?"* ≠ *"cho tôi xem doanh thu tháng 7"*.

### Hai tầng tra cứu

```
Câu hỏi
  ↓
[Tầng 1] Chuẩn hoá → IntentHash → tra DB (chính xác, < 5ms)
  ↓ miss
[Tầng 2] Embedding → Qdrant collection `plan_templates` → cosine ≥ 0.90
  ↓ miss
Lập plan mới (như Stage 10)
```

### Tầng 1 — Chuẩn hoá ý định

```python
def intent_hash(question: str, module: str) -> str:
    """Chuẩn hoá câu hỏi thành khoá ổn định, BỎ tham số cụ thể."""
    text = question.lower().strip()

    # Bỏ dấu câu, chuẩn hoá khoảng trắng
    text = re.sub(r"[^\w\sàáâãèéêìíòóôõùúýăđĩũơưạảấầẩẫậắằẳẵặẹẻẽếềểễệ]", " ", text)
    text = re.sub(r"\s+", " ", text)

    # Thay tham số bằng placeholder — đây là bước quan trọng nhất
    text = re.sub(r"\b\d{1,2}[/-]\d{1,2}([/-]\d{2,4})?\b", "<ngay>", text)
    text = re.sub(r"\btháng\s+\d{1,2}\b", "<thang>", text)
    text = re.sub(r"\bquý\s+[1-4iv]+\b", "<quy>", text)
    text = re.sub(r"\bnăm\s+\d{4}\b", "<nam>", text)
    text = re.sub(r"\b\d+\b", "<so>", text)

    # Bỏ từ đệm không mang ý nghĩa phân biệt
    for filler in ("cho tôi", "cho mình", "xem", "giúp", "vui lòng", "hãy", "ạ", "nhé"):
        text = text.replace(filler, " ")
    text = re.sub(r"\s+", " ", text).strip()

    return hashlib.sha256(f"{module}|{text}".encode()).hexdigest()
```

Nhờ bước thay placeholder, *"doanh thu tháng 7"* và *"doanh thu tháng 6"* cho **cùng** `IntentHash`
— đúng như mong muốn, vì plan giống nhau, chỉ khác tham số.

### Tầng 2 — Ngữ nghĩa qua Qdrant

Collection mới `plan_templates` (thêm vào Stage 12.3):
```python
{
    "template_id": "uuid",
    "canonical_question": "Báo cáo tồn kho tuần này",
    "module": "inventory",
    "required_permissions": ["Permissions.Warehouse.InventoryReportManagement.View"],
    "required_tools": ["get_low_stock_products", "get_inventory_report"],
    "status": "active",
    "success_rate": 0.94,
}
```

Ngưỡng `0.90` cao hơn ngưỡng tìm sản phẩm (`0.55` ở Stage 12.5) — **cố ý**. Cache miss chỉ tốn
thêm một lần lập plan; cache hit sai làm AI chạy kế hoạch cho câu hỏi khác, tệ hơn nhiều.

---

## 19.4. Tham số hoá plan

Plan lưu dưới dạng template có slot, không phải giá trị cụ thể:

```jsonc
{
  "slots": [
    {"name": "from_date", "type": "date",   "描述": "Ngày bắt đầu kỳ báo cáo"},
    {"name": "to_date",   "type": "date",   "description": "Ngày kết thúc kỳ báo cáo"},
    {"name": "category",  "type": "string", "description": "Danh mục", "optional": true}
  ],
  "stepsTemplate": [
    {
      "id": "s1", "order": 1,
      "title": "Lấy danh sách sản phẩm tồn kho thấp",
      "detail": "Gọi get_low_stock_products, danh mục {{category}}",
      "expectedTools": ["get_low_stock_products"]
    },
    {
      "id": "s2", "order": 2,
      "title": "Tính giá trị tồn kho từ {{from_date}} đến {{to_date}}",
      "detail": "Gọi get_inventory_report cho kỳ {{from_date}} → {{to_date}}",
      "expectedTools": ["get_inventory_report"]
    }
  ]
}
```

### Điền slot — bằng model rẻ, không phải agent đầy đủ

```python
async def fill_slots(template: dict, question: str, server_date: str) -> dict:
    """Chỉ trích xuất tham số, KHÔNG lập lại kế hoạch."""
    schema = build_slot_schema(template["slots"])
    prompt = render("fill_plan_slots",
                    question=question,
                    slots=json.dumps(template["slots"], ensure_ascii=False),
                    today=server_date)          # server_date từ backend — Stage 16.2
    llm = get_llm(model=settings.model, temperature=0, max_output_tokens=200)
    return await (llm.with_structured_output(schema)).ainvoke(prompt)
```

~200 token thay vì 3.000. Đây là nơi phần lớn khoản tiết kiệm đến từ.

> **Không để sidecar tự tính "hôm nay"** — `server_date` phải do backend cấp theo GMT+7,
> theo đúng quy định Stage 16.2. Cache plan không được phá vỡ quy tắc này.

---

## 19.5. Luồng hoàn chỉnh

```
Câu hỏi cần plan (classify_node của Stage 10.6)
   ↓
Tra cache: IntentHash → Qdrant
   ↓
┌─ MISS ─────────────────────────────────────┐
│ Lập plan như Stage 10                      │
│ User duyệt (có thể sửa)                    │
│ Thực thi                                   │
│ Nếu THÀNH CÔNG và user KHÔNG sửa nhiều     │
│   → tham số hoá và lưu thành template      │
└────────────────────────────────────────────┘
   ↓
┌─ HIT ──────────────────────────────────────┐
│ 1. Kiểm tra hiệu lực (19.6)                │
│ 2. Điền slot bằng Model (max_tokens thấp)  │
│ 3. Hiện plan card, ghi nhãn "kế hoạch có sẵn"│
│ 4. User duyệt / sửa / huỷ  ← VẪN BẮT BUỘC  │
│ 5. Thực thi                                │
│ 6. Cập nhật thống kê template               │
└────────────────────────────────────────────┘
```

### Vẫn phải để user duyệt

Cache plan tiết kiệm **thời gian suy nghĩ**, không bỏ qua **quyền quyết định**. Plan tái dùng
vẫn hiện đầy đủ và vẫn cần duyệt.

**Ngoại lệ có điều kiện — chế độ tin cậy:** template có `UseCount ≥ 10`,
`SuccessCount/UseCount ≥ 0.95`, `UserEditCount = 0`, và **toàn bộ tool là chỉ-đọc**
→ cho phép tự động duyệt, hiện thông báo:
> ⚡ Dùng kế hoạch quen thuộc (đã chạy 14 lần) — [xem kế hoạch]

Có tool ghi thì **không bao giờ** tự duyệt, bất kể thống kê tốt đến đâu (Stage 13.5).

---

## 19.6. Vô hiệu hoá cache — phần dễ sai nhất

Cache plan lỗi thời nguy hiểm hơn không có cache. Sáu điều kiện vô hiệu:

| # | Điều kiện | Cách phát hiện | Hành động |
|---|---|---|---|
| 1 | Tool trong template bị gỡ/deprecated | So `RequiredTools` với `TOOL_SPECS` (Stage 17) | `Status = stale`, lập plan mới |
| 2 | Tool đổi schema (`version` tăng) | So `ToolRegistryFingerprint` | Như trên |
| 3 | User không đủ permission của template | So `RequiredPermissions` với context | Bỏ qua template này, không xoá |
| 4 | `GLOSSARY.md` đổi định nghĩa liên quan | Hash file glossary lưu kèm template | `Status = stale` |
| 5 | Tỉ lệ user sửa cao (`UserEditCount/UseCount > 0.3`) | Thống kê | `Status = disabled`, cần review |
| 6 | Không dùng 90 ngày | `LastUsedAt` | Xoá (dọn rác) |

**Kiểm tra hiệu lực chạy ở bước 1 của luồng HIT**, trước khi điền slot:

```python
async def validate_template(tpl: dict, context: dict) -> bool:
    if tpl["status"] != "active":
        return False
    if tpl["toolRegistryFingerprint"] != registry_fingerprint():
        # Registry đổi — kiểm tra kỹ từng tool thay vì loại thẳng
        for name in tpl["requiredTools"]:
            spec = TOOL_SPECS.get(name)
            if spec is None or spec.status != "active":
                await backend.mark_template_stale(tpl["id"])
                return False
    user_perms = set(context.get("permissions") or [])
    if not set(tpl["requiredPermissions"]).issubset(user_perms):
        return False                      # không xoá — user khác vẫn dùng được
    return True
```

Lưu ý điểm 3: **không xoá** template khi user thiếu quyền — chỉ bỏ qua. Template vẫn hợp lệ với
người có quyền.

---

## 19.7. Học từ plan user đã sửa

Khi user sửa plan (Stage 10.4), thông tin đó rất giá trị: **plan gốc của AI chưa tốt**.

```
User sửa plan → thực thi thành công
   ↓
Ghi nhận: template gốc UserEditCount++
   ↓
Nếu cùng một sửa đổi lặp ≥ 3 lần cho cùng template
   → tạo template PHIÊN BẢN MỚI từ plan đã sửa
   → template cũ Status = disabled
```

Ví dụ: 3 người dùng đều sửa bước "So sánh với quý trước" thành "So sánh cùng kỳ năm ngoái"
→ hệ thống học và mặc định dùng "cùng kỳ năm ngoái".

**Chỉ học từ plan chạy thành công.** Plan bị user huỷ (`RejectCount`) không được đưa vào template
— đó là plan sai, học vào là nhân bản lỗi.

---

## 19.8. Chống nhiễm cache

| Rủi ro | Phòng ngừa |
|---|---|
| Cache plan từ run **thất bại** | Chỉ lưu template khi run `Completed` **và** không có tool nào lỗi |
| Cache plan chứa dữ liệu cụ thể của user | Tham số hoá bắt buộc; **test** kiểm tra `StepsTemplate` không chứa tên riêng, mã đơn, số tiền |
| Template của module A dùng cho câu hỏi module B | `IntentHash` gồm `module`; Qdrant filter theo `module` |
| Template rò rỉ thông tin qua `CanonicalQuestion` | Câu hỏi có thể chứa PII → chạy `_scrub_text()` (Stage 11.3) trước khi lưu |
| Một user "dạy" template xấu cho cả hệ thống | Cần ≥ 3 lần lặp từ ≥ 2 user khác nhau mới tạo template mới |
| Template phình to vô kiểm soát | Trần 500 template; vượt thì xoá theo `LastUsedAt` cũ nhất |

> Ô thứ hai là rủi ro nghiêm trọng nhất: nếu template lưu nguyên *"Lấy đơn hàng của khách Nguyễn
> Văn A, mã DH-2026-001"* thì user khác tái dùng sẽ thấy dữ liệu không phải của mình.
> Test này là **bắt buộc**, không phải nên có.

---

## 19.9. Đo lường

| Chỉ số | Mục tiêu sau 1 tháng |
|---|---|
| Cache hit rate | ≥ 40% các run cần plan |
| Token tiết kiệm/ngày | Theo dõi xu hướng |
| Thời gian lập plan (hit vs miss) | < 0.5s vs 3–6s |
| Tỉ lệ user sửa plan tái dùng | ≤ 15% (cao hơn = template kém) |
| Tỉ lệ user huỷ plan tái dùng | ≤ 5% |
| Số template `active` | 20–80 (quá ít = khoá quá chặt, quá nhiều = chuẩn hoá kém) |
| Số template `stale` do tool đổi | Theo dõi — cao là dấu hiệu tool đổi quá thường xuyên |

Ghi `planCacheHit: bool` và `templateId` vào metadata LangSmith (Stage 6.6) và log run
để đối chiếu được.

---

## 19.10. Rủi ro

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Hit sai → chạy kế hoạch cho câu hỏi khác | **Cao** | Ngưỡng cosine 0.90; `IntentHash` gồm module; user vẫn duyệt |
| Template lỗi thời sau khi đổi tool | **Cao** | 6 điều kiện vô hiệu ở 19.6, gắn với fingerprint của Stage 17 |
| Template chứa dữ liệu của user khác | **Cao** | Tham số hoá + test bắt buộc (19.8) |
| Plan cứng nhắc, không thích nghi ngữ cảnh mới | Trung bình | User sửa được; học từ sửa đổi (19.7) |
| Tăng độ phức tạp hệ thống | Trung bình | Mặc định bật; tắt bằng env `PLAN_CACHE_ENABLED=false` khi cần |
| Chuẩn hoá quá mạnh → gộp hai ý định khác nhau | Trung bình | Review thủ công template có `UseCount` cao; eval riêng |

**Cờ tắt khẩn cấp:** đặt env `PLAN_CACHE_ENABLED=false` trên sidecar.
Mặc định bật khi feature có mặt — không cần khai trong `appsettings.json`.
Tắt → về đúng hành vi Stage 10, không mất tính năng gì.

---

## Definition of Done — Stage 19

> **Trạng thái (2026-08-04):** Đường ĐỌC (tra cache, validate, điền slot, render bước, hiện plan
> để duyệt) đã xong và có test. Đường GHI (tự học template mới sau khi run thành công, học từ sửa
> đổi lặp lại, chế độ tự duyệt) **chưa làm** — xem lý do ở mục cuối.

- [x] Migration `ChatPlanTemplate` chạy được trên **cả 3 provider** (SqlServer/MySQL/PostgreSQL —
      `add-migration.ps1` tạo cả 3, không chỉ 2 như bản nháp đầu của tài liệu này).
- [x] Collection Qdrant `plan_templates` tạo được (`qdrant_client.py::ensure_collections`).
      **Chưa thêm payload index riêng cho `module`/`status`** (lọc vẫn đúng qua `Filter`, chỉ chưa
      tối ưu tốc độ lọc — nên làm khi số template đủ lớn để cần).
- [x] `intent_hash` chuẩn hoá đúng: "doanh thu tháng 7" và "doanh thu tháng 6" cho **cùng** hash
      (`test_intent_hash_thang_khac_nhau_cho_cung_hash`).
- [x] Hỏi cùng một loại câu hỏi lần 2 → cache hit, **bỏ qua hoàn toàn bước gọi LLM sinh plan**
      (`test_plan_node_cache_hit_bo_qua_llm_sinh_plan`). Chưa đo mốc "< 0.5s" trên môi trường thật.
- [x] Slot được điền bằng `Model` với `max_output_tokens=200`, dùng `server_date` từ state (backend
      cấp qua `ChatRequest.server_date`, sidecar không tự tính — `plan_cache.fill_slots`).
- [x] Plan tái dùng **vẫn hiện ra và vẫn cần duyệt** — luồng cache-hit vẫn gọi `add_plan_step` +
      `mark_plan_ready` y hệt luồng thường, không có bước tự động chuyển `Approved`.
- [ ] Chế độ tự duyệt cho template đủ điều kiện (toàn tool chỉ-đọc) — **chưa làm**, xem ghi chú cuối.
- [x] Gỡ một tool / đổi fingerprint → `validate_plan_template()` kiểm tra lại từng tool trong
      `requiredTools`, coi cache miss nếu tool không còn active
      (`test_validate_plan_template_chan_khi_tool_bi_go`).
- [ ] Đổi `GLOSSARY.md` → template liên quan chuyển `stale` — **chưa làm** (chưa có cơ chế hash
      glossary gắn vào template).
- [x] User thiếu quyền → `validate_plan_template()` trả `False` (bỏ qua template, không xoá)
      (`test_validate_plan_template_chan_khi_thieu_quyen`).
- [x] **Test: `contains_hardcoded_data()` chặn mã đơn/SĐT/số tiền cụ thể trong `StepsTemplate`**
      (`test_contains_hardcoded_data_*`, 4 test). **Guard đã có nhưng chưa có nơi gọi nó** — vì
      bước "tự lưu template mới sau khi lập plan thành công" chưa được xây (xem ghi chú cuối).
- [ ] Plan bị user huỷ hoặc run thất bại → không được lưu thành template — **chưa áp dụng được**,
      vì chưa có luồng lưu template nào cả.
- [ ] Cùng một sửa đổi lặp 3 lần từ ≥ 2 user → sinh template phiên bản mới — **chưa làm**.
- [x] `planCacheHit`/`templateId` phát ra qua event `plan_cache_hit` (chưa nối vào LangSmith
      metadata riêng — Stage 6.6 chưa làm phần đó).
- [x] Cờ `PLAN_CACHE_ENABLED` (`plan_cache_enabled` trong config) → khi `false`,
      `plan_node` bỏ qua toàn bộ khối tra cache, về đúng hành vi Stage 10.
- [ ] Hit rate ≥ 40% sau 1 tháng chạy thật — cần dữ liệu thật để đo, không đo được ở giai đoạn này.

### Vì sao đường GHI chưa làm

Tự động tham số hoá (biến bước đã render cụ thể — ví dụ "báo cáo từ 1/6 đến 30/6" — ngược lại
thành template có `{{from_date}}`/`{{to_date}}`) là bài toán tách biệt, khó hơn chiều ngược lại
(điền slot vào template có sẵn) mà tài liệu Stage 19 không nêu thuật toán cụ thể. Một bản làm vội
có rủi ro thật: parameterize sai có thể để lọt dữ liệu cụ thể của một user (tên khách, mã đơn) vào
template dùng chung cho user khác — đúng rủi ro nghiêm trọng nhất mà mục 19.8 cảnh báo. Thay vì
làm ẩu, đã dừng ở: có sẵn `contains_hardcoded_data()` làm lưới an toàn, cần nối vào khi ai đó xây
bước tự học (hoặc một công cụ admin tạo template thủ công).
