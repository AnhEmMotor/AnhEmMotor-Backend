# Stage 02 — Bộ tool AI cho khách hàng (dùng chung sidecar)

> Ưu tiên: 🔴 Cao · Ước lượng: 2–3 ngày · Phụ thuộc: **Stage 01**
> Mục tiêu: AI trả lời bằng dữ liệu sản phẩm thật, trả về card 2 cấp (sản phẩm → biến thể màu), và
> **không** thể chạm được bất kỳ tool nội bộ nào của Manager Chat.

---

## 2.1. Kiến trúc: 1 sidecar, 2 persona

Quyết định đã chốt: dùng chung tiến trình `AISidecar/` thay vì tách sidecar riêng. Cách làm an toàn:

```
AISidecar/app/
  main.py
  routers/
    manager_chat.py   ← đã có, persona "manager", nạp catalog 71+ tool nội bộ
    store_chat.py      ← MỚI, persona "store", nạp catalog RIÊNG chỉ ~5-6 tool đọc-only
  tools/
    registry.py         ← đã có, đọc SharedConfig/chat-tools-catalog.json
```

**Nguyên tắc bắt buộc — đây là điểm rủi ro bảo mật chính của cả tính năng:**

1. Persona `store` được khởi tạo với **danh sách tool cố định, tách biệt hoàn toàn** khỏi persona
   `manager` — không load toàn bộ registry rồi lọc bằng permission runtime (dễ lỗi, một dòng code sai
   là lộ hết). Thay vào đó, `store_chat.py` chỉ import đúng danh sách tool cho phép, không có đường nào
   để LLM router của Stage 20 (`chatbot-ai`) mở rộng scope sang tool khác.
2. Endpoint backend mà các tool này gọi ngược vào là **controller mới**, không phải
   `InternalChatToolsController` — xem 2.2.
3. Test khoá lại (xem Definition of Done): request qua route `store_chat.py` gọi thử tên tool nội bộ
   (`get_staff_performance`, `search_customers`...) phải bị từ chối ở tầng router, không phải bị chặn
   nhờ may mắn ở tầng permission.

---

## 2.2. Endpoint backend mới: `PublicChatToolsController`

`WebAPI/Controllers/PublicChatToolsController.cs` — song song với `InternalChatToolsController.cs`
nhưng:

- `[LocalhostOnly]` giữ nguyên (chỉ sidecar gọi được — sidecar và backend chạy cùng máy/mạng nội bộ,
  khách hàng không bao giờ gọi thẳng controller này).
- **Không** có `[HasPermission(...)]` trên từng action (khác hẳn `InternalChatToolsController` — ở đó
  permission kiểm tra nhân viên gọi có quyền xem module nào; ở đây không có nhân viên đứng sau request,
  toàn bộ action đều là dữ liệu công khai-an toàn theo thiết kế, không có khái niệm "khách hàng có
  quyền gì").
- Action nào **không nằm trong danh sách dưới** thì không được thêm vào controller này, kể cả khi tiện
  tay copy từ `InternalChatToolsController` — mỗi action mới ở đây phải tự hỏi "lộ cho khách xem có sao
  không" trước khi thêm.

### Danh sách tool ban đầu (tái dùng handler đã có, viết action mới gọi lại handler cũ)

| Tool | Handler tái dùng | Field ẩn bớt so với bản nội bộ |
|---|---|---|
| `search_products` | `SearchProductsForChatQueryHandler` | Giữ nguyên — DTO vốn đã không có giá vốn |
| `get_product_detail` | `GetProductDetailForChatQueryHandler` | Giữ nguyên — `ChatProductDetailDto`/`ChatProductVariantDetailDto` đã an toàn |
| `get_product_stock` | `GetProductStockForChatQueryHandler` | Chỉ trả `còn hàng / hết hàng / còn ít`, **không** trả số lượng tồn chính xác (số tồn kho chính xác là dữ liệu vận hành nội bộ) |
| `get_product_price_list` | `GetProductPriceListForChatQueryHandler` | Giữ nguyên |
| `list_brands` | Handler tương ứng đã có trong `InternalChatToolsController` | Giữ nguyên |

Viết action mới trong `PublicChatToolsController` gọi lại đúng `IRequest` hiện có qua `sender.Send(...)`
(tái dùng handler, không copy logic) — chỉ khác action wrapper và có thể map lại DTO để ẩn field ở
`get_product_stock`. Nếu cần DTO ẩn field riêng, tạo `ChatProductStockPublicDto` mới thay vì sửa
`ChatProductStockDto` hiện có (sửa DTO chung sẽ ảnh hưởng ngược lại Manager Chat).

**Không thêm tool ghi dữ liệu nào ở persona `store`** (không tạo đơn, không tạo lead qua chat ở giai
đoạn này) — nếu sau này cần, đó là quyết định riêng phải đi qua rà soát bảo mật + Plan Mode như tool ghi
của Manager Chat, không tự ý thêm ở Stage này.

---

## 2.3. Card 2 cấp trả về cho FE

Sidecar trả JSON lines như cơ chế stream đã có (Quyết định 5 ở `chatbot-ai/00-OVERVIEW.md`), thêm 1
loại event mới bên cạnh `text`/`thinking`:

```jsonc
{ "kind": "product-cards", "items": [
  { "productId": 123, "name": "Honda SH 2024", "imageUrl": "...", "priceFrom": 89000000, "priceTo": 95000000 }
]}
{ "kind": "variant-cards", "productId": 123, "items": [
  { "variantId": 456, "colorName": "Đỏ đen", "sku": "SH24-RB", "price": 91000000 }
]}
```

FE (Stage 03) render 2 loại card này thay vì bong bóng text thuần. `variant-cards` chỉ xuất hiện sau
khi khách bấm vào 1 `product-card` cụ thể hoặc khi AI xác định rõ khách muốn xem màu — không đẩy toàn
bộ biến thể ngay từ câu hỏi đầu tiên (tránh rợp thông tin).

Lưu `CardsJson` (đã thêm ở `StoreChatMessage` Stage 01) = nguyên văn payload này khi ghi tin nhắn AI,
để Stage 06 render lại y hệt trong transcript quản trị mà không cần tính lại.

---

## 2.4. Fuzzy / tự do ngôn ngữ

Không cần xây thêm gì mới ở tầng backend — `SearchProductsForChatQueryHandler` đã có fallback bỏ dấu
(test `Search_KeywordCoDauNhungTenSanPhamThieuDau_VanTimDuocQuaFallback`). Việc còn lại là ở
system prompt của persona `store`: hướng dẫn LLM tự trích từ khoá sản phẩm từ câu tự do của khách (kể
cả câu chứa lỗi chính tả/ngữ pháp) rồi gọi `search_products` với `keyword` đã trích, thay vì yêu cầu
khách gõ đúng cú pháp.

---

## Definition of Done — Stage 02

- [ ] `PublicChatToolsController` tồn tại, `[LocalhostOnly]`, chỉ chứa đúng 5 tool liệt kê ở mục 2.2 —
      không có action nào thừa.
- [ ] Test: gọi thử tên tool nội bộ (vd. `get_staff_performance`) từ persona `store` → bị từ chối ở
      tầng router sidecar (400/lỗi rõ ràng), không rơi vào nhánh nào gọi được
      `InternalChatToolsController`.
- [ ] `get_product_stock` qua route công khai không trả số lượng tồn kho chính xác.
- [ ] Gõ từ khoá sai/thiếu dấu → vẫn trả đúng sản phẩm (test parity ≥ 3 case, xem 00-OVERVIEW mục
      "Kiểm chứng khi thực thi").
- [ ] AI trả về đúng cấu trúc `product-cards` → bấm 1 card → trả `variant-cards` đúng sản phẩm đó.
- [ ] Không tool ghi dữ liệu nào tồn tại ở persona `store`.
