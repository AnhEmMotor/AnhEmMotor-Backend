# Stage 12 — Qdrant & RAG

> Yêu cầu #1 · Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 4–5 ngày · Phụ thuộc: **Stage 3, 7**
> Mục tiêu: AI tìm kiếm được theo **ngữ nghĩa** — hiểu "xe ga tiết kiệm xăng cho nữ" thay vì
> chỉ khớp từ khoá, và trả lời được từ tài liệu nội bộ (bảo hành, chính sách, hướng dẫn).

> **⚠️ Nợ từ Stage 18 (Consistency) — làm kèm khi xong Stage này:**
> - **18.7** (trích dẫn RAG có mã `citationId`, kiểm chứng được) — chunk trả về từ Qdrant phải
>   kèm `citationId`; prompt bắt buộc gắn mã `[c1]`; output guard chặn mã bịa.
> - **18.3** (dọn checkpoint mồ côi) — nếu Stage này đổi checkpointer sang lưu bền (Postgres/Redis)
>   thay cho `MemorySaver` hiện tại, tiện làm luôn job dọn checkpoint mồ côi + endpoint
>   `POST /internal/chat/runs/exists`.
>
> Xem chi tiết: [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md), mục 18.3, 18.7.

---

## 12.1. Vì sao cần Qdrant khi đã có tool `search_products`

Tool ở Stage 3 gọi thẳng SQL — chỉ khớp `LIKE '%keyword%'`. Không xử lý được:

| Câu hỏi | SQL `LIKE` | Vector search |
|---|---|---|
| "Xe ga tiết kiệm xăng cho nữ" | ❌ không có sản phẩm nào tên vậy | ✅ khớp SH Mode, Vision, Lead |
| "Xe nào giống Air Blade nhưng rẻ hơn" | ❌ | ✅ hiểu tương đồng |
| "Chính sách đổi trả trong bao lâu?" | ❌ không nằm trong bảng sản phẩm | ✅ tìm trong tài liệu |
| "Nhớt cho xe côn tay 150cc" | ⚠️ phụ thuộc từ khoá chính xác | ✅ |

**Kết luận:** giữ cả hai. SQL cho truy vấn chính xác (mã đơn, tồn kho, giá), vector cho truy vấn
mô tả. Xem chiến lược kết hợp ở mục 12.6.

---

## 12.2. Hạ tầng Qdrant

### Triển khai
Thêm vào `docker-compose.yml` (tạo mới nếu chưa có, tham khảo `SETUP_VPS.md`):

```yaml
services:
  qdrant:
    image: qdrant/qdrant:v1.12.4
    restart: unless-stopped
    ports:
      - "127.0.0.1:6333:6333"     # chỉ localhost, KHÔNG expose ra ngoài
      - "127.0.0.1:6334:6334"     # gRPC
    volumes:
      - qdrant_storage:/qdrant/storage
    environment:
      QDRANT__SERVICE__API_KEY: ${QDRANT_API_KEY}
      QDRANT__LOG_LEVEL: WARN

volumes:
  qdrant_storage:
```

**Bảo mật bắt buộc:**
- Bind `127.0.0.1`, không mở firewall. Qdrant mặc định **không có auth** — mở ra internet là
  cho không toàn bộ dữ liệu.
- Đặt `QDRANT__SERVICE__API_KEY` qua biến môi trường / GitHub Secrets.
- Backup volume `qdrant_storage` cùng lịch backup DB.

### Cấu hình phía ứng dụng
`WebAPI/appsettings.json`:
```jsonc
"AISetup": {
    // ... các khoá hiện có, KHÔNG đổi Model
    "QdrantUrl": "",              // ví dụ http://localhost:6333
    "QdrantApiKey": "",
    "EmbeddingModel": "text-embedding-004"
}
```
`AiSidecarManager.cs` truyền xuống env (đã liệt kê ở Stage 7.3).

> **Mặc định RAG bật** khi `QdrantUrl` có giá trị. Nếu Qdrant chết hoặc không cấu hình URL,
> tool knowledge tự gỡ khỏi registry — chatbot vẫn hoạt động bình thường bằng SQL.
> Không để Qdrant thành điểm chết đơn lẻ. Tắt khẩn cấp bằng env `RAG_ENABLED=false` trên sidecar.

---

## 12.3. Thiết kế collection

| Collection | Nội dung | Nguồn | Tần suất cập nhật |
|---|---|---|---|
| `product_catalog` | Mỗi sản phẩm 1 điểm: tên + mô tả + thương hiệu + danh mục + thuộc tính | Bảng `Products` | Realtime khi CRUD |
| `knowledge_base` | Tài liệu nội bộ chia đoạn: bảo hành, đổi trả, hướng dẫn, FAQ | File markdown trong `docs/knowledge/` | Thủ công khi sửa tài liệu |
| `plan_templates` *(Stage 19)* | Kế hoạch đã dùng, tham số hoá | `ChatPlanTemplate` | Khi plan chạy thành công |
| `tool_catalog` *(tuỳ chọn)* | Mô tả tool, để gợi ý nhóm khi router lỗi | `TOOL_SPECS` | Khi đổi registry |
| `chat_memory` *(giai đoạn 2)* | Tóm tắt các phiên chat cũ của user | `ChatSession` | Sau khi session kết thúc |

> **`tool_catalog` là tuỳ chọn có phạm vi hẹp**, không phải đường chính để chọn tool.
> Nó chỉ dùng ở hai chỗ: thay fail-safe tĩnh khi router lỗi, và gợi ý tool khi model bịa tên.
> Lý do không dùng làm chính ở quy mô 71 tool (câu tiếp nối và cặp tool gần giống nhau):
> xem [20-STAGE-DYNAMIC-TOOL-SCOPING.md](20-STAGE-DYNAMIC-TOOL-SCOPING.md) mục 20.9.

### Payload schema — `product_catalog`

```python
{
    "product_id": "uuid",
    "name": "Honda SH 150i ABS 2026",
    "brand": "Honda",
    "brand_id": 1,
    "category": "Xe máy",
    "category_id": 3,
    "vehicle_type": "Xe ga",
    "price": 98000000,
    "in_stock": True,
    "stock_quantity": 12,
    "colors": ["Đỏ", "Đen"],
    "is_active": True,          # ← LỌC BẮT BUỘC, xem bên dưới
    "updated_at": "2026-07-26T00:00:00Z",
}
```

**Payload index bắt buộc** (Qdrant lọc nhanh hơn nhiều khi có index):
```python
for field, schema in [
    ("brand_id", "integer"), ("category_id", "integer"),
    ("price", "float"), ("in_stock", "bool"), ("is_active", "bool"),
]:
    client.create_payload_index("product_catalog", field, schema)
```

> **Bẫy thường gặp:** sản phẩm bị xoá/ẩn trong SQL nhưng còn trong Qdrant → AI giới thiệu hàng
> không còn bán. **Luôn lọc `is_active = true`** trong mọi truy vấn, và cho phép reindex toàn bộ
> (mục 12.5) để chữa lệch dữ liệu.

### Vector config
```python
VectorParams(
    size=768,                    # khớp text-embedding-004
    distance=Distance.COSINE,
)
```
> **Chốt trước khi index:** đổi embedding model = phải reindex **toàn bộ**. Chọn một lần rồi giữ.
> `text-embedding-004` của Google là lựa chọn tự nhiên vì đã dùng Gemini (cùng API key).

---

## 12.4. Ingestion — đẩy dữ liệu vào Qdrant

### Kiến trúc: .NET là nguồn sự thật, sidecar là nơi index

```
Product CRUD (.NET)
   ↓ MediatR notification: ProductChangedNotification
IndexProductHandler  →  hàng đợi (Channel)
   ↓
ProductIndexWorker (BackgroundService)
   ↓ POST /internal/index/products  (sidecar)
Sidecar: sinh embedding → upsert Qdrant
```

**Vì sao qua sidecar chứ không gọi Qdrant trực tiếp từ .NET:** embedding model nằm ở Python,
và giữ toàn bộ logic AI ở một chỗ (nguyên tắc Stage 7).

### Endpoint sidecar
```
POST /internal/index/products      { items: [...] }       # upsert theo lô
POST /internal/index/products/delete { productIds: [...] }
POST /internal/index/knowledge     { documents: [...] }
POST /internal/index/rebuild       { collection: "..." }   # reindex toàn bộ
```
Tất cả đều yêu cầu `X-Internal-Secret` (Stage 1.4).

### Xử lý theo lô
- Gom tối đa **100 sản phẩm/lần** gọi embedding — giảm số request tới Google API.
- Có backoff khi bị rate limit (`429`).
- Job **reindex toàn bộ** chạy hằng đêm để chữa lệch dữ liệu do lỗi/mất event.

> ⚠️ **Reindex dở dang để lại collection thiếu dữ liệu mà không ai biết.** Bắt buộc:
> reindex vào **collection tạm** (`product_catalog_v2`), verify số điểm khớp số bản ghi SQL,
> rồi mới đổi alias sang collection mới. Qdrant hỗ trợ alias — dùng nó thay vì ghi trực tiếp
> lên collection đang phục vụ.

### Text để embed
Không embed nguyên JSON. Dựng câu mô tả tự nhiên:
```python
def build_product_text(p: dict) -> str:
    parts = [
        p["name"],
        f"Thương hiệu {p['brand']}" if p.get("brand") else "",
        f"Danh mục {p['category']}" if p.get("category") else "",
        f"Loại xe {p['vehicle_type']}" if p.get("vehicle_type") else "",
        f"Màu {', '.join(p['colors'])}" if p.get("colors") else "",
        p.get("description", "")[:800],
    ]
    return ". ".join(x for x in parts if x)
```

### Knowledge base
- Đặt tài liệu ở `AnhEmMotor-Backend/docs/knowledge/*.md` (versioned trong git).
- Chia đoạn theo heading, mỗi đoạn 300–600 token, chồng lấn 50 token.
- Payload giữ `source_file`, `heading`, `chunk_index` để **trích dẫn nguồn** trong câu trả lời.

> ⚠️ **Trích dẫn phải verify được, không để AI tự nêu tên tài liệu.** Mỗi chunk trả về kèm
> `citationId` (`c1`, `c2`...); prompt buộc AI gắn `[c1]` sau câu; output guard chặn mã không tồn tại.
> Không có cơ chế này thì AI sẽ nói *"theo chính sách bảo hành"* trong khi đoạn thật lấy từ tài liệu
> đổi trả. Xem [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.7.

---

## 12.5. Tool mới ở sidecar

`app/tools/knowledge.py`:

```python
class SemanticProductSearchInput(BaseModel):
    query: str = Field(description=(
        "Mô tả nhu cầu bằng ngôn ngữ tự nhiên, ví dụ: "
        "'xe ga tiết kiệm xăng cho nữ', 'nhớt cho xe côn tay 150cc'"
    ))
    max_price: Optional[int] = Field(default=None, description="Giá tối đa (VNĐ)")
    in_stock_only: bool = Field(default=True, description="Chỉ lấy hàng còn tồn")
    limit: int = Field(default=8, description="Số kết quả, tối đa 15")


class KnowledgeSearchInput(BaseModel):
    query: str = Field(description=(
        "Câu hỏi về chính sách, bảo hành, đổi trả, quy trình nội bộ"
    ))
    limit: int = Field(default=5)
```

Mô tả tool phải **phân định rõ với tool SQL**, nếu không agent sẽ chọn nhầm:

```python
description=(
    "Tìm sản phẩm theo MÔ TẢ, NHU CẦU hoặc ĐẶC ĐIỂM khi người dùng không nêu tên "
    "chính xác. Ví dụ: 'xe ga tiết kiệm xăng', 'đồ bảo hộ đi mưa'. "
    "KHÔNG dùng tool này khi người dùng đã nêu tên/mã sản phẩm cụ thể — "
    "khi đó hãy dùng search_products."
)
```

### Truy vấn có lọc

```python
async def semantic_product_search(query: str, max_price=None,
                                  in_stock_only=True, limit=8) -> dict:
    vector = await embed(query)

    must = [FieldCondition(key="is_active", match=MatchValue(value=True))]
    if in_stock_only:
        must.append(FieldCondition(key="in_stock", match=MatchValue(value=True)))
    if max_price:
        must.append(FieldCondition(key="price", range=Range(lte=max_price)))

    hits = await qdrant.search(
        collection_name="product_catalog",
        query_vector=vector,
        query_filter=Filter(must=must),
        limit=min(limit, 15),
        score_threshold=0.55,        # cắt kết quả quá xa nghĩa
    )
    return {
        "items": [format_hit(h) for h in hits],
        "totalCount": len(hits),
        "source": "semantic",
    }
```

> **`score_threshold` rất quan trọng.** Không có nó, Qdrant luôn trả đủ `limit` kết quả kể cả khi
> chẳng cái nào liên quan → AI giới thiệu sản phẩm vớ vẩn với vẻ tự tin. Hiệu chỉnh giá trị này
> bằng bộ câu hỏi mẫu (mục 12.8).

### Giá và tồn kho — luôn lấy lại từ SQL

Payload trong Qdrant có thể cũ vài phút. **Không bao giờ báo giá/tồn kho từ payload Qdrant.**
Sau khi vector search ra danh sách `product_id`, gọi tool SQL lấy giá và tồn kho hiện tại:

```
semantic_product_search → [product_id...] → get_products_by_ids (SQL) → dữ liệu chính xác
```
Gộp 2 bước này **trong cùng một tool** để agent không phải gọi 2 lần (tối ưu số vòng, Stage 14).

---

## 12.6. Chiến lược kết hợp SQL + Vector

| Loại câu hỏi | Đường đi |
|---|---|
| Có tên/mã cụ thể ("SH 150i", "DH-2026-001") | SQL trực tiếp |
| Mô tả nhu cầu ("xe ga cho nữ") | Vector → lấy id → SQL bù dữ liệu |
| Hỏi chính sách / quy trình | Vector trên `knowledge_base` |
| Số liệu tổng hợp (doanh thu, tồn kho) | SQL, **không bao giờ** dùng vector |
| Mơ hồ | Vector trước, nếu điểm cao nhất < ngưỡng thì fallback SQL `LIKE` |

**Hybrid search (nâng cao, làm sau nếu cần):** Qdrant hỗ trợ sparse vector (BM25) kết hợp dense
vector với RRF fusion. Cải thiện rõ với tên riêng và mã sản phẩm. Chỉ làm sau khi đo được
dense-only chưa đủ tốt trên bộ câu hỏi mẫu.

---

## 12.7. Chống rò rỉ dữ liệu qua vector

Qdrant **không biết** hệ thống phân quyền của bạn. Nếu index dữ liệu nhạy cảm, ai chat cũng
có thể moi ra qua câu hỏi khéo léo.

**Quy tắc:**
1. **Chỉ index dữ liệu không nhạy cảm:** catalog sản phẩm (vốn công khai trên `AnhEmMotor-Store`)
   và tài liệu nội bộ chung. **Không index** đơn hàng, thông tin khách hàng, doanh thu, nhân sự.
2. Nếu về sau phải index dữ liệu có phân quyền → thêm field payload `required_permission` và
   **luôn** đưa vào filter dựa trên permission lấy từ context (Stage 3), không tin prompt.
3. Kết quả từ `knowledge_base` vẫn phải chạy qua redaction (Stage 11) và guardrail chống
   prompt injection gián tiếp (Stage 13) — tài liệu có thể chứa nội dung do người khác soạn.

---

## 12.8. Đo chất lượng

Không có cách nào biết RAG tốt hay không ngoài việc đo. Tạo `AISidecar/evals/rag_cases.yaml`:

```yaml
- query: "xe ga tiết kiệm xăng cho nữ"
  must_include_any: ["Vision", "SH Mode", "Lead"]
  must_not_include: ["Winner", "Exciter"]

- query: "chính sách đổi trả trong bao lâu"
  collection: knowledge_base
  must_include_any: ["đổi trả", "7 ngày"]

- query: "abcxyz không tồn tại"
  expect_empty: true          # kiểm tra score_threshold hoạt động
```

Metric theo dõi: **Recall@5** và **tỉ lệ trả rác** (trả kết quả khi lẽ ra phải rỗng).
Chạy lại mỗi khi đổi `score_threshold`, đổi cách dựng text embed, hoặc đổi embedding model.

---

## 12.8b. Test (không phải eval)

Eval ở 12.8 đo **chất lượng kết quả**. Nhưng phần lớn logic ở Stage này là code thường,
test được bằng `pytest` mà không cần gọi LLM.

`AISidecar/tests/test_qdrant.py` — dùng `respx` mock Qdrant HTTP, không cần Qdrant thật:

```python
async def test_luon_loc_is_active():
    """Sản phẩm đã xoá/ẩn không được lọt vào kết quả — bẫy ở 12.3."""
    captured = capture_search_filter()
    await semantic_product_search("xe ga", in_stock_only=False)
    conditions = {c.key for c in captured.must}
    assert "is_active" in conditions, "thiếu filter is_active"


async def test_score_threshold_cat_ket_qua_rac():
    """Không có threshold thì Qdrant luôn trả đủ limit dù chẳng cái nào liên quan."""
    mock_hits([hit(score=0.31), hit(score=0.28)])
    result = await semantic_product_search("abcxyz không tồn tại")
    assert result["items"] == []
    assert result["totalCount"] == 0


async def test_gia_va_ton_kho_lay_lai_tu_sql():
    """Payload Qdrant có thể cũ — giá/tồn kho PHẢI lấy lại từ SQL (12.5)."""
    mock_qdrant_payload(product_id="p1", price=90_000_000, stock=0)
    mock_sql_response(product_id="p1", price=98_000_000, stock=12)
    result = await semantic_product_search("xe ga")
    assert result["items"][0]["price"] == 98_000_000
    assert result["items"][0]["stockQuantity"] == 12


async def test_rag_tat_thi_khong_co_tool_knowledge():
    """QdrantUrl rỗng hoặc RAG_ENABLED=false → chatbot vẫn chạy, chỉ mất tool semantic (12.2)."""
    settings.qdrant_url = ""
    names = {t.name for t in build_tools(admin_context())}
    assert "search_knowledge" not in names
    assert "semantic_product_search" not in names
    assert "search_products" in names          # SQL vẫn còn


async def test_reindex_dung_alias_khong_ghi_de_collection_dang_phuc_vu():
    """Reindex phải vào collection tạm rồi mới đổi alias (12.4)."""
    calls = await run_reindex("product_catalog")
    assert any(c.collection.endswith("_v2") for c in calls.upserts)
    assert calls.alias_switched_after_verify is True


async def test_ingest_theo_lo_toi_da_100():
    calls = await index_products([make_product(i) for i in range(250)])
    assert all(len(c.items) <= 100 for c in calls)
    assert sum(len(c.items) for c in calls) == 250


async def test_khong_index_du_lieu_nhay_cam():
    """Đơn hàng / khách hàng KHÔNG được index (12.7)."""
    from app.services.qdrant_client import INDEXED_COLLECTIONS
    forbidden = {"orders", "customers", "payroll", "revenue", "debt"}
    assert not (forbidden & {c.lower() for c in INDEXED_COLLECTIONS})


def test_build_product_text_khong_nhet_json():
    text = build_product_text({"name": "Honda SH 150i", "brand": "Honda",
                               "colors": ["Đỏ"], "description": "x" * 2000})
    assert "{" not in text and "}" not in text
    assert "Honda SH 150i" in text
    assert len(text) < 1200                     # description bị cắt ở 800
```

**Test .NET** — `UnitTests/ChatIndexing.cs`: CRUD sản phẩm phát `ProductChangedNotification`;
`ProductIndexWorker` gom lô và gọi đúng endpoint sidecar; sản phẩm xoá mềm gọi endpoint `delete`.

> **Phân biệt:** 7 test trên chạy mọi commit, không tốn tiền, không cần Qdrant thật.
> Eval ở 12.8 chạy tay khi đổi `score_threshold` / embedding model. Không gộp hai thứ.

---

## 12.9. Chi phí & hiệu năng

| Hạng mục | Ước tính | Ghi chú |
|---|---|---|
| Embedding lần đầu | ~1 request / 100 sản phẩm | Một lần, rẻ |
| Embedding khi CRUD | 1 request / sản phẩm thay đổi | Gom lô, debounce 5s |
| Embedding câu hỏi | 1 request / lượt search | **Cache theo hash câu hỏi**, TTL 1 giờ |
| RAM Qdrant | ~4KB/điểm với 768 chiều | 10.000 sản phẩm ≈ 40MB — rất nhẹ |
| Latency search | 5–20ms | Không đáng kể so với LLM |

**Cache embedding câu hỏi** là tối ưu đáng giá nhất: câu hỏi lặp lại nhiều, mỗi lần tiết kiệm
~100ms và 1 API call. Dùng `functools.lru_cache` cho bản đơn giản, Redis nếu multi-instance.

---

## Definition of Done — Stage 12

- [x] Qdrant chạy, chỉ bind `127.0.0.1`, có API key, volume được backup —
      `docker-compose.yml` (root) + hướng dẫn ở `SETUP_VPS.md` Bước 7. **Chưa chạy thật trên VPS,
      chỉ mới cấu hình** — cần `docker compose up -d qdrant` trên môi trường thật.
- [x] Collection `product_catalog` + `knowledge_base` tạo được, có payload index —
      `app/services/qdrant_client.py::ensure_collections()`, gọi tự động lúc sidecar khởi động
      (`app/main.py`, chỉ khi `QdrantUrl` có giá trị). Idempotent — chỉ tạo collection còn thiếu.
- [x] Toàn bộ sản phẩm hiện có đã index — `ProductIndexWorker` (BackgroundService) nhận
      `ProductChangedNotification` (phát từ `UnitOfWork.SaveChangesAsync`, không phải từng
      command handler) → gom lô ≤100 → gọi `/internal/index/products`.
      **Job reindex toàn bộ (`/internal/index/rebuild` + alias switch) có code (`reindex_products`)
      nhưng chưa có job lịch chạy hằng đêm — cần thêm nếu muốn tự động.**
- [x] CRUD sản phẩm → Qdrant cập nhật — qua `ProductIndexWorker`, debounce 5s/batch 100
      (chưa đo thời gian thực tế "trong vòng 30 giây" trên môi trường thật).
- [x] Xoá/ẩn sản phẩm → không còn xuất hiện trong kết quả tìm kiếm — soft-delete vẫn kích hoạt
      `ProductChangedNotification` (EF ChangeTracker thấy `Modified`), re-index với `isActive=false`,
      `search_products()` luôn lọc `is_active=true`.
- [x] Hỏi "xe ga tiết kiệm xăng cho nữ" → trả về sản phẩm hợp lý, kèm **giá và tồn kho lấy từ SQL** —
      `semantic_product_search` (`app/tools/knowledge.py`) gộp vector search + `products/detail` +
      `products/stock`. **Chưa chạy thử với dữ liệu thật/Qdrant thật.**
- [x] Hỏi "chính sách đổi trả" → trả lời từ knowledge base, **có trích dẫn nguồn** —
      `search_knowledge` trả `citationId`; guard chặn mã bịa (`tool_guard.check_output`); FE render
      chip `[c1]` bấm mở được (`ChatDrawer.vue`).
- [x] Hỏi câu vô nghĩa → trả rỗng, AI nói không tìm thấy, không bịa —
      `score_threshold=0.55` cắt kết quả xa nghĩa (`qdrant_client.py`).
- [x] `QdrantUrl` rỗng hoặc `rag_enabled=false` → chatbot vẫn chạy bình thường bằng SQL —
      `build_all_tools()` gỡ 2 tool qua Qdrant khi tắt (test `test_rag_tat_thi_khong_co_tool_knowledge`).
- [x] `tests/test_qdrant.py` — 10 test (respx/fake-client, không cần Qdrant thật) pass.
- [x] `UnitTests/ChatIndexing.cs` — notification → queue, pass. **Chưa có test EF-ChangeTracker cho
      `UnitOfWork.SaveChangesAsync` (cần tier IntegrationTests, không phải UnitTests — xem ghi chú).**
- [ ] Bộ eval RAG chạy được, Recall@5 ≥ 80% trên tập câu hỏi mẫu — `evals/rag_cases.yaml` đã có,
      **chưa chạy được vì cần Qdrant thật + dữ liệu đã index**.
- [x] Xác nhận không có dữ liệu đơn hàng / khách hàng nào bị index —
      `INDEXED_COLLECTIONS` allowlist + test `test_khong_index_du_lieu_nhay_cam`.
