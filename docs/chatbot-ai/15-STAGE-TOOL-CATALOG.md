# Stage 15 — Danh mục Tool đầy đủ cho toàn bộ dự án

> Yêu cầu bổ sung · Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 15–22 ngày (chia 5 đợt; P4 +4–6 ngày, P5 +3–4 ngày)
> Phụ thuộc: **Stage 3** (hạ tầng tool calling), **Stage 13 + 20** (guardrails & tool scoping —
> bắt buộc xong trước khi rollout P1), **Stage 16** (envelope & parity test)

Stage 3 làm 5–6 tool mẫu để chứng minh hạ tầng chạy. Stage này **quét toàn bộ dự án** và
lập danh mục tool phủ hết các phân hệ nghiệp vụ.

> **Cập nhật 2026-07-31 — mở rộng tool ghi dữ liệu (2 đợt):** bản đầu chỉ có 1 tool ghi
> (`create_purchase_request`).
> - **Đợt 1 (P4, mục 15.8):** +12 tool ghi (Create/Update) → tổng 71 → 83.
> - **Đợt 2 (P5, mục 15.9):** người dùng hỏi "có thêm được tool xoá và chỉnh sửa không" — đã quyết
>   định **KHÔNG thêm hard delete** (giữ nguyên bất biến từ đầu), chỉ thêm **soft-delete có tool khôi
>   phục đối xứng** (`deactivate_X`/`restore_X`) + 4 tool chỉnh sửa mở rộng (khách hàng, lead, sản
>   phẩm, lịch hẹn) → +8 tool, tổng **71 → 91**.
>
> Quyết định #8 ở [00-OVERVIEW.md](00-OVERVIEW.md) mục 5 đổi từ "Không ở bản đầu" thành **"Có, mở
> rộng theo 2 đợt P4+P5"** — vẫn giữ nguyên tắc bất biến: **không tool xoá vĩnh viễn nào được cấp cho
> AI** (chỉ soft-delete có khôi phục), mọi tool ghi đều `is_write: true` và phải qua Plan Mode + xác
> nhận người dùng + audit log + idempotency key (Stage 13.5), không có ngoại lệ. Sau P4+P5, ba module
> `customer`, `service`, `inventory` đã **chạm đúng trần 10 tool/module** — không còn dư địa mở rộng
> thêm mà không tách module con mới (xem 15.2 cuối mục).
>
## 0. Tiến độ thực thi — đọc mục này TRƯỚC khi quay lại Stage 15

> Cập nhật lần cuối: 2026-07-31. Mục này tồn tại để **quay lại sau vẫn biết ngay cái gì đã làm, cái
> gì chưa** — không cần đọc lại toàn bộ doc hay đoán từ code. Cập nhật mục này mỗi khi trạng thái
> đổi (đừng để nó lệch với thực tế).

**Tổng quan nhanh: 69/91 tool đã có code thật, build sạch, test xanh. 22 tool còn lại chưa viết
code (2 bị chặn bởi phụ thuộc ngoài, 20 mới ở mức đặc tả).**

### ✅ Đã xong (có code thật, đã build + test)
- **P1 (20 tool)** — toàn bộ đọc, phủ sản phẩm/tồn kho/đơn hàng/doanh thu/khách hàng/sửa chữa/bảo
  hành/vận chuyển. File: `SharedConfig/chat-tools-catalog.json`, `WebAPI/Controllers/InternalChatToolsController.cs`.
- **P2 (41/43 tool)** — đọc mở rộng: kho chi tiết, NCC, hợp đồng, marketing, tài chính, nhân sự, logistics.
- **P3 (8 tool)** — nhạy cảm (lương G4/G5, admin J4/J5) + tool ghi đầu tiên `create_purchase_request` (B9).
- Test đã cập nhật khớp: [`UnitTests/ChatToolCatalog.cs`](../../UnitTests/ChatToolCatalog.cs) (đếm
  69), [`AISidecar/tests/test_chat_tools.py`](../../AISidecar/tests/test_chat_tools.py),
  [`AISidecar/tests/test_chat_tools_catalog.py`](../../AISidecar/tests/test_chat_tools_catalog.py).
- `dotnet build` toàn solution: 0 lỗi. `dotnet test` (UnitTests+ControllerTests+IntegrationTests):
  1,184/1,184 pass. `pytest` AISidecar: toàn bộ pass (gồm 2 test trần bất biến
  `test_khong_module_nao_vuot_tran`, `test_hai_module_bat_ky_khong_vuot_tran_request`).
- **2026-07-31 (sau đó):** đổi 7 tool `*_detail` từ nhận ID sang nhận `keyword` (tên/SĐT/biển số/tên
  NCC) — xem nguyên tắc mới ở mục 15.2 #7. Đã sửa xong `get_order_status`, `get_shipment_tracking`,
  `get_repair_order_detail`, `get_warranty_claim_detail`, `get_lead_detail`,
  `get_purchase_request_detail`, `get_inventory_receipt_detail` — build + test xanh lại.

### ⛔ Bị chặn — không phải việc chưa làm, mà là phụ thuộc ngoài chưa tồn tại
- `semantic_product_search` (A3) — cần Qdrant, Stage 12 chưa xây.
- `search_knowledge` (K1) — cần Qdrant, Stage 12 chưa xây.
- → Khi Stage 12 xong, quay lại 2 tool này trước (đã có sẵn vị trí trong catalog/module, chỉ cần code).

### 📝 Đã đặc tả đầy đủ, CHƯA viết code (việc cần làm tiếp theo, ưu tiên theo thứ tự)
1. **P4 — 12 tool ghi (mục 15.8):** `update_purchase_request_status` (B10), `update_order_status`
   (C11), `create_voucher` (C12), `create_lead` (D8), `update_lead_status` (D9),
   `create_booking_appointment` (E12), `update_repair_order_status` (E13), `create_warranty_claim`
   (E14), `record_expense` (F9), `confirm_debt_payment` (F10), `update_shipment_status` (H6),
   `update_store_setting` (J6). Mỗi tool đã có mô tả DÙNG KHI/KHÔNG DÙNG KHI đầy đủ, chỉ cần
   implement theo khuôn 9 bước (16.11) + 3 gate P4 (cuối 15.8).
2. **P5 — 8 tool (mục 15.9):** `update_product_info` (A8), `update_customer_profile` (D10),
   `update_lead_info` (D11), `update_booking_appointment` (E15), `deactivate_voucher` (C13),
   `restore_voucher` (C14), `deactivate_news` (D12), `restore_news` (D13). Đã có mô tả đầy đủ +
   4 gate P5 (cuối 15.9) — quan trọng nhất: **mọi tool xoá phải làm cùng lúc với tool khôi phục**.

### 🚫 Đã quyết định KHÔNG làm (đừng đề xuất lại trừ khi có lý do mới)
- **Không có tool xoá vĩnh viễn (hard delete)** nào, ở bất kỳ đợt nào — quyết định chốt 2026-07-31
  khi được hỏi trực tiếp. Lý do: AI có thể bị prompt injection/hiểu nhầm/xác nhận nhầm; xoá vĩnh viễn
  sai thì không revert được. Nếu sau này thật sự cần xoá cứng cho một entity cụ thể, phải quay lại
  hỏi rõ lý do nghiệp vụ và đánh giá riêng, không mặc định thêm.

### ⏭️ Việc KHÔNG thuộc phạm vi code tool, cần làm riêng trước khi bật thật (xem DoD từng đợt)
- Parity test + contract test snapshot riêng từng tool (bước 7-8 của khuôn 9 bước).
- Đặt cờ `shadow` cho tool mới trong config runtime thật của môi trường deploy.
- Bộ eval câu hỏi thật (≥60 câu P1, ≥120 câu P2) và ma trận permission 6 vai trò.
- Test end-to-end xác nhận Plan Mode thực sự chặn tool ghi chưa confirm (B9 chưa được chạy thử thật).
- Rà soát thủ công tool nhạy cảm (F1, G4, G5, J5) trước khi merge.

---

## 15.1. Kết quả quét dự án

Số liệu thật từ `AnhEmMotor-Backend`:

| Hạng mục | Số lượng |
|---|---|
| Nhóm feature (`Application/Features/`) | **57** |
| Controller V1 (`WebAPI/Controllers/V1/`) | **~50** |
| Module phân quyền (`Domain/Constants/Permission/Modules.cs`) | **6** |
| Nhóm permission | **~50** |
| Hằng số permission | **185** |

### 6 module phân quyền — xương sống của danh mục tool

| Module | Hằng số | Nhóm permission |
|---|---|---|
| `Permissions.Admin` | `Modules.Admin` | ContractManagement, DashboardManagement, EmployeeManagement, FileManagement, FinanceContractManagement, PayrollManagement, RoleManagement, SettingManagement, UserManagement |
| `Permissions.Warehouse` | `Modules.Warehouse` | DebtPaymentManagement, InventoryReportManagement, InventorySettingManagement, LedgerManagement, ProductManagement, PurchaseRequestManagement, ReceiptManagement, SupplierManagement |
| `Permissions.Order` | `Modules.Order` | ContractManagement, CustomerManagement, CustomerSelection, DraftOrderManagement, OrderManagement, ProductManagement, ProductSelection, SalesInvoiceManagement |
| `Permissions.Accountant` | `Modules.Accountant` | ContractManagement, ContractVerification, CustomerManagement, DashboardManagement, DebtPaymentManagement, EmployeeManagement, PayrollManagement, SupplierContractManagement |
| `Permissions.Factory` | `Modules.Factory` | BookingAppointmentManagement, BookingManagement, ContractManagement, CustomerManagement, CustomerSelection, DashboardManagement, RepairOrderManagement, SparePartSelection |
| `Permissions.Marketing` | `Modules.Marketing` | BannerManagement, BookingManagement, ContactManagement, CustomerAssetManagement, CustomerCareManagement, CustomerManagement, CustomerVoucherManagement, LeadManagement, NewsManagement |

> **Ánh xạ gần, không phải 1-1:** 6 module phân quyền này là **điểm khởi đầu** cho nhóm router ở
> [13-STAGE-GUARDRAILS.md](13-STAGE-GUARDRAILS.md) mục 13.3. Nhưng vì trần 10 tool/module
> (13.3c), nhóm router phải mịn hơn — cuối cùng thành **13 nhóm + `none`**, xem mục 15.2.
>
> Cấu trúc phân quyền vẫn là thứ nên tận dụng để **suy ra** `required_permissions` của từng tool
> — chỉ riêng việc *gom nhóm cho router* thì cần chia nhỏ hơn.

---

## 15.2. Nguyên tắc thiết kế danh mục

1. **Chỉ đọc trong đợt P1 và P2.** Tool ghi để đợt P3 và P4, và phải qua Plan Mode + confirm
   (Stage 13.5) — không có ngoại lệ, kể cả tool "update trạng thái" tưởng như an toàn.
2. **Tool ánh xạ theo *câu hỏi*, không theo *endpoint*.** 57 feature × ~8 query = ~450 query,
   nhưng chỉ cần ~91 tool (71 đọc/gộp cơ bản + 12 ghi ở 15.8 + 8 tool ở 15.9). Gộp theo ý định người dùng.
3. **Mỗi tool khai báo đủ:** `required_permissions`, `is_write`, `module`, `summarizer` (Stage 11.3).
4. **Không tạo tool cho:** export file, upload, sitemap, template import, audit log, endpoint
   nội bộ của FE (`GetActiveVariantLiteListFor*`, `GetOrderStatusTransitionMap`...).
   Chúng phục vụ UI, không phải câu hỏi của người dùng.
5. **Giới hạn bản ghi:** mặc định 10, trần 25 (50 với danh sách nhân sự).
6. **Tối đa 10 tool/module**, và **mọi cặp 2 module ≤ 20 tool** (trần một request).
   Đây là **bất biến có test chặn**, không phải hướng dẫn —
   xem [13-STAGE-GUARDRAILS.md](13-STAGE-GUARDRAILS.md) mục 13.3b và 13.3c.
7. **Không tool nào nhận ID nội bộ (khoá chính số nguyên) làm tham số bắt buộc từ người dùng.**
   Người dùng cuối không biết và không quan tâm ID trong DB — họ nhớ tên khách hàng, SĐT, biển số
   xe, tên nhà cung cấp. Mọi tool "chi tiết theo ID" (`*_detail`, `get_*_by_id`...) phải nhận
   `keyword` (string) và tự tìm kiếm nội bộ theo trường định danh tự nhiên, trả về **danh sách khớp
   (tối đa 5)** thay vì bắt AI/người dùng phải biết ID trước — nếu 0 kết quả, trả `Items: []`,
   KHÔNG trả lỗi. Áp dụng ngay (2026-07-31) cho 7 tool đã có: `get_order_status`,
   `get_shipment_tracking`, `get_repair_order_detail`, `get_warranty_claim_detail`,
   `get_lead_detail`, `get_purchase_request_detail`, `get_inventory_receipt_detail`. Khi viết P4/P5
   (15.8/15.9), các tool `update_*`/`get_*_detail` còn lại (state B10, C11, D9, D11, E13, E15...)
   **phải thiết kế theo nguyên tắc này ngay từ đầu**, không viết theo ID rồi sửa lại sau.

### Phân bổ module — đã tách lại để không vượt trần

Bản nháp đầu của danh mục có `service` = 11 tool và `inventory` = 11 tool, **vượt trần**.
Đã tách thành 13 module:

| Module | Nhóm tool (đọc) | Ghi P4 (15.8) | Ghi P5 (15.9) | Số tool |
|---|---|---|---|---|
| `product` | A1–A6 | — | A8 | 7 |
| `supplier` | A7, I1, I2 | — | — | 3 |
| `inventory` | B1–B9 | B10 | — | 10 |
| `sales` | C1–C8 | C11 | — | 9 |
| `contract` | C9, F6, F7 | — | — | 3 |
| `customer` | D1–D6 | D8, D9 | D10, D11 | **10** |
| `marketing` | C10, D7 | C12 | C13, C14, D12, D13 | 7 |
| `service` | E1, E2, E6–E9, E11 | E12, E13 | E15 | **10** |
| `warranty` | E3–E5, E10 | E14 | — | 5 |
| `finance` | F1–F5, F8 | F9, F10 | — | 8 |
| `hr` | G1–G5 | — | — | 5 |
| `logistics` | H1–H5 | H6 | — | 6 |
| `admin` | J1–J5 | J6 | — | 6 |
| `knowledge` | K1, K2 | — | — | 2 |
| **Tổng** | | | | **91** |

Mỗi tool thuộc **đúng một** module — `A7` (`get_supplier_prices_for_variant`) chuyển sang
`supplier` nên `product` còn 6 (+1 ở P5 = 7), không phải 7 từ đầu.

**⚠️ Sau khi thêm P4+P5: `customer`, `service` và `inventory` đều chạm ĐÚNG trần 10 tool/module.**
Đây là giới hạn cứng, không phải gợi ý — **không còn chỗ để thêm bất kỳ tool nào (đọc hay ghi) vào
3 module này** trừ khi trước đó có tool bị gộp/loại bỏ để nhường chỗ. Cặp module lớn nhất sau khi
mở rộng: `inventory`+`service` = **20**, `customer`+`service` = **20**, `inventory`+`sales` = 19 —
đều **chạm đúng hoặc gần chạm trần 20/cặp**. Nếu cần thêm tool nữa vào các module này về sau, phải
**tách module con mới** (giống cách `service`/`inventory` từng được tách ở 15.2) — **không được nới
`MAX_TOOLS_PER_MODULE`/`MAX_TOOLS_PER_REQUEST`**, vì đó là chữa triệu chứng chứ không phải chữa gốc.
`test_tool_registry.py::test_khong_module_nao_vuot_tran` và
`test_hai_module_bat_ky_khong_vuot_tran_request` **phải chạy lại và xanh** trước khi merge bất kỳ
tool nào ở 15.8/15.9 vào catalog thật.

> **Lưu ý:** module của **tool** không nhất thiết trùng module **phân quyền** của dự án.
> `contract` gom hợp đồng bán / tài chính / nhà cung cấp lại vì người dùng hỏi về chúng theo cùng
> một cách, dù chúng thuộc ba module permission khác nhau. Permission vẫn kiểm tra theo hằng số
> gốc — module tool chỉ để router chọn nhóm.
>
> Router ở 13.3 phải cập nhật danh sách nhóm từ 8 lên **14** (13 module + `none`).

---

## 15.3. Danh mục tool

Ký hiệu: **P1** = đợt 1 (giá trị cao nhất) · **P2** = đợt 2 · **P3** = đợt 3 (tool ghi)
`R` = chỉ đọc · `W` = ghi dữ liệu

### A. Sản phẩm & Danh mục — module `product`

| # | Tool | Đợt | R/W | Nguồn (Features) | Permission gốc |
|---|---|---|---|---|---|
| A1 | `search_products` | P1 | R | `Products/GetProductsListForManager` | `Order.ProductManagement.View` hoặc `Warehouse.ProductManagement.View` |
| A2 | `get_product_detail` | P1 | R | `Products/GetProductById`, `GetVariantLiteByProductId` | như trên |
| A3 | `semantic_product_search` | P2 | R | Qdrant (Stage 12) | như trên |
| A4 | `get_product_price_list` | P2 | R | `Products/GetProductsListForPriceManagement` | `Order.ProductManagement.View` |
| A5 | `list_brands` | P2 | R | `Brands/GetBrandsList`, `GetBrandStatistics` | `Warehouse.ProductManagement.View` |
| A6 | `list_categories` | P2 | R | `ProductCategories/GetProductCategoriesList`, `GetProductCategoryStats` | như trên |
| A7 | `get_supplier_prices_for_variant` | P2 | R | `ProductQuotations/GetSupplierPricesForVariant` | `Warehouse.SupplierManagement.View` |
| A8 | `update_product_info` | **P5** | **W** | `Products/Commands` (giá bán + thông tin cơ bản, gộp chung 1 tool) | `Warehouse.ProductManagement.Update` |

> **Gộp quan trọng:** `search_products` trả luôn **giá + tồn kho + biến thể**, tránh agent phải
> gọi thêm 2 tool (Stage 14.2b). **A8** gộp sửa giá và sửa thông tin cơ bản vào 1 tool duy nhất
> (không tách `update_product_price`/`update_product_info` riêng) — đúng nguyên tắc 15.2 #2, tránh
> phình thêm tool gần giống nhau vào module `product` vốn đã gần trần.

### B. Kho & Tồn kho — module `inventory`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| B1 | `get_stock_on_hand` | P1 | R | `InventoryOnHand`, `Statistical/GetProductStockAndPrice` | `Warehouse.InventoryReportManagement.View` |
| B2 | `get_low_stock_products` | P1 | R | `InventoryReports/GetInventoryReportSummary` | như trên |
| B3 | `get_inventory_report` | P1 | R | `InventoryReports/GetInventoryReportSummary`, `GetInventoryReportDetail` | như trên |
| B4 | `get_inventory_ledger` | P2 | R | `InventoryLedgers/GetInventoryLedger` | `Warehouse.LedgerManagement.View` |
| B5 | `list_inventory_receipts` | P2 | R | `InventoryReceipts/GetInventoryReceiptsList`, `GetInventoryReceiptStats` | `Warehouse.ReceiptManagement.View` |
| B6 | `get_inventory_receipt_detail` | P2 | R | `InventoryReceipts/GetInventoryReceiptById` | như trên |
| B7 | `list_purchase_requests` | P2 | R | `PurchaseRequests/GetPurchaseRequests`, `GetApprovedPurchaseRequests` | `Warehouse.PurchaseRequestManagement.View` |
| B8 | `get_purchase_request_detail` | P2 | R | `PurchaseRequests/GetPurchaseRequestById` | như trên |
| B9 | `create_purchase_request` | **P3** | **W** | `PurchaseRequests/Commands` | `Warehouse.PurchaseRequestManagement.Create` |
| B10 | `update_purchase_request_status` | **P4** | **W** | `PurchaseRequests/Commands` (approve/reject) | `Warehouse.PurchaseRequestManagement.Update` *(xác nhận tên hằng số thật khi implement)* |

### C. Bán hàng & Đơn hàng — module `sales`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| C1 | `get_sales_summary` | P1 | R | `Statistical/GetDailyRevenue`, `GetMonthlyRevenueProfit` | `Admin.DashboardManagement.View` / `Accountant.DashboardManagement.View` |
| C2 | `get_order_status` | P1 | R | `Outputs/GetOutputById` | `Order.OrderManagement.View` |
| C3 | `list_orders` | P1 | R | `Outputs/GetOutputsList` | như trên |
| C4 | `get_order_statistics` | P1 | R | `Order/GetOrderStatistics`, `Statistical/GetOrderStatusCounts` | như trên |
| C5 | `get_top_selling_products` | P1 | R | `Statistical/GetAdminProductReport`, `GetProductReportLastMonth` | `Admin.DashboardManagement.View` |
| C6 | `get_revenue_by_category` | P2 | R | `Statistical/GetRevenueByCategory`, `GetDailyCategoryRevenue` | như trên |
| C7 | `get_sales_report` | P2 | R | `SalesReports/GetSalesReport` | `Order.OrderManagement.View` |
| C8 | `get_recent_transactions` | P2 | R | `Statistical/GetRecentTransactions` | `Accountant.DashboardManagement.View` |
| C9 | `list_sales_contracts` | P2 | R | `SalesContracts/GetSalesContractsList`, `GetSalesContractStatistics` | `Order.ContractManagement.View` |
| C10 | `list_vouchers` | P2 | R | `Vouchers/GetVoucherList` | `Marketing.CustomerVoucherManagement.View` |
| C11 | `update_order_status` | **P4** | **W** | `Outputs/Commands` (confirm/cancel/mark shipped) | `Order.OrderManagement.Update` *(xác nhận tên hằng số thật)* |
| C12 | `create_voucher` | **P4** | **W** | `Vouchers/Commands` | `Marketing.CustomerVoucherManagement.Create` |
| C13 | `deactivate_voucher` | **P5** | **W** | `Vouchers/Commands` (soft-delete, set `DeletedAt`/ngừng hiệu lực) | `Marketing.CustomerVoucherManagement.Delete` |
| C14 | `restore_voucher` | **P5** | **W** | `Vouchers/Commands` (khôi phục) | `Marketing.CustomerVoucherManagement.Delete` *(hoặc `.Update` nếu tách riêng)* |

> **Gộp:** `get_sales_summary` nhận `compare_with_previous: bool` để trả cả kỳ trước trong 1 lần gọi.
> ⚠️ **C11** chỉ cho phép chuyển sang tập trạng thái an toàn đã whitelist (ví dụ xác nhận, hủy khi
> chưa xuất kho) — KHÔNG cho AI tự do set mọi `StatusId`, tránh đẩy đơn vào trạng thái nghiệp vụ
> không hợp lệ. ⚠️ **C12** tạo voucher có tác động tài chính trực tiếp — bắt buộc giới hạn
> `discount_value`/`max_usage` trong khoảng an toàn ở tầng Command .NET (không chỉ dựa vào AI).
> **C13/C14 là cặp xoá mềm + khôi phục bắt buộc đi cùng nhau** (nguyên tắc 15.9) — `deactivate_voucher`
> chỉ set `DeletedAt`/`IsActive=false`, không xoá bản ghi DB thật, luôn khôi phục được qua `restore_voucher`
> hoặc từ UI quản trị.

### D. Khách hàng & Marketing — module `customer`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| D1 | `get_customer_profile` | P1 | R | `Customer/GetCustomerProfile360` | `Order.CustomerManagement.View` / `Marketing.CustomerManagement.View` |
| D2 | `search_customers` | P1 | R | `Customer`, `Users` | như trên |
| D3 | `get_lead_pipeline` | P2 | R | `Leads/GetLeadPipeline`, `GetLeads` | `Marketing.LeadManagement.View` |
| D4 | `get_lead_detail` | P2 | R | `Leads/GetLeadById` | như trên |
| D5 | `list_contacts` | P2 | R | `Contacts/GetContacts`, `GetPaginatedContacts` | `Marketing.ContactManagement.View` |
| D6 | `get_loyalty_members` | P2 | R | `Loyalty/GetLoyaltyMembers` | `Marketing.CustomerCareManagement.View` |
| D7 | `list_news` | P3 | R | `News`, `NewsCategories` | `Marketing.NewsManagement.View` |
| D8 | `create_lead` | **P4** | **W** | `Leads/Commands` | `Marketing.LeadManagement.Create` |
| D9 | `update_lead_status` | **P4** | **W** | `Leads/Commands` (chuyển giai đoạn pipeline) | `Marketing.LeadManagement.Update` |
| D10 | `update_customer_profile` | **P5** | **W** | `Customer/Commands` (sửa thông tin liên hệ/ghi chú) | `Order.CustomerManagement.Update` |
| D11 | `update_lead_info` | **P5** | **W** | `Leads/Commands` (sửa thông tin liên hệ/xe quan tâm, khác `update_lead_status` chỉ đổi giai đoạn) | `Marketing.LeadManagement.Update` |
| D12 | `deactivate_news` | **P5** | **W** | `News/Commands` (soft-delete, set `DeletedAt`) | `Marketing.NewsManagement.Delete` |
| D13 | `restore_news` | **P5** | **W** | `News/Commands` (khôi phục, clear `DeletedAt`) | `Marketing.NewsManagement.Delete` *(hoặc permission `.Update` nếu tách riêng)* |

> ⚠️ **D1, D2 trả PII** (tên, số điện thoại, địa chỉ). Bắt buộc chạy qua redaction của
> [11-STAGE-REASONING-TRANSPARENCY.md](11-STAGE-REASONING-TRANSPARENCY.md) — panel suy nghĩ **không**
> được hiện thông tin khách hàng thô ở Production.
> ⚠️ **D10 sửa dữ liệu PII trực tiếp** — bắt buộc xác nhận lại thông tin đã đổi trước khi ghi (đọc
> lại cho người dùng nghe/xem), không chỉ dựa vào 1 câu xác nhận mơ hồ.
> **D12/D13 (`deactivate_news`/`restore_news`) là cặp bắt buộc đi cùng nhau** — không tool xoá nào
> trong danh mục này được thêm mà thiếu tool khôi phục tương ứng (nguyên tắc mới từ 2026-07-31,
> xem 15.9). Đây vẫn là soft-delete (`DeletedAt`), **không phải hard delete**.

### E. Dịch vụ, Sửa chữa & Bảo hành — module `service`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| E1 | `list_repair_orders` | P1 | R | `RepairOrders/GetRepairOrdersList` | `Factory.RepairOrderManagement.View` |
| E2 | `get_repair_order_detail` | P1 | R | `RepairOrders/GetRepairOrderDetail` | như trên |
| E3 | `list_warranty_claims` | P1 | R | `WarrantyClaims/GetWarrantyClaimsList` | `Factory.RepairOrderManagement.View` |
| E4 | `get_warranty_claim_detail` | P2 | R | `WarrantyClaims/GetWarrantyClaimDetail`, `GetWarrantyHistory` | như trên |
| E5 | `get_warranty_terms` | P2 | R | `WarrantyTerms/GetWarrantyTermsList`, `GetWarrantyTermById` | như trên |
| E6 | `list_booking_appointments` | P1 | R | `BookingAppointments/GetBookingAppointments` | `Factory.BookingAppointmentManagement.View` |
| E7 | `list_bookings` | P2 | R | `Bookings/GetBookings` | `Factory.BookingManagement.View` |
| E8 | `list_services` | P2 | R | `Services/GetServicesList` | `Factory.RepairOrderManagement.View` |
| E9 | `get_workshop_dashboard` | P2 | R | `Statistical/GetWorkshopDashboardOverview` | `Factory.DashboardManagement.View` |
| E10 | `list_workshop_payments` | P2 | R | `WorkshopPayments/GetWorkshopPaymentsList`, `GetWorkshopPaymentStatistics` | `Factory.RepairOrderManagement.View` |
| E11 | `get_vehicle_portfolio` | P2 | R | `Vehicles/GetVehiclePortfolio`, `GetVehicles` | `Factory.CustomerManagement.View` |
| E12 | `create_booking_appointment` | **P4** | **W** | `BookingAppointments/Commands` | `Factory.BookingAppointmentManagement.Create` |
| E13 | `update_repair_order_status` | **P4** | **W** | `RepairOrders/Commands` | `Factory.RepairOrderManagement.Update` |
| E14 | `create_warranty_claim` | **P4** | **W** | `WarrantyClaims/Commands` | `Factory.RepairOrderManagement.Create` *(hoặc hằng số riêng WarrantyClaim nếu có)* |
| E15 | `update_booking_appointment` | **P5** | **W** | `BookingAppointments/Commands` (đổi giờ hẹn HOẶC huỷ, gộp 1 tool qua tham số `action`) | `Factory.BookingAppointmentManagement.Update` |

> ⚠️ **E13** chỉ whitelist các bước chuyển trạng thái hợp lệ trong workflow sửa chữa (không cho AI
> nhảy cóc trạng thái). **E14** tạo yêu cầu bảo hành mới — vẫn cần nhân viên xưởng thẩm định thực tế
> sau đó, tool chỉ ghi nhận yêu cầu ban đầu, không tự động phê duyệt bồi hoàn. **E15 gộp reschedule
> và huỷ vào 1 tool** (tham số `action: "reschedule" | "cancel"`) thay vì tách 2 tool riêng — module
> `service` đã chạm trần 10 sau khi thêm, không còn chỗ cho tool gần giống nhau (15.2 #2 + #6).
> Huỷ lịch hẹn qua E15 vẫn là **soft-cancel** (đổi `Status`), không xoá bản ghi.

### F. Tài chính & Công nợ — module `finance`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| F1 | `get_pnl_report` | P1 | R | `Statistical/GetPnlReport` | `Accountant.DashboardManagement.View` |
| F2 | `get_suppliers_with_debt` | P1 | R | `DebtPayments/GetSuppliersWithDebt` | `Accountant.DebtPaymentManagement.View` |
| F3 | `get_supplier_debt_detail` | P2 | R | `DebtPayments/GetSupplierDebtLogs`, `GetReceiptsWithDebtBySupplierId` | như trên |
| F4 | `list_expenses` | P2 | R | `Expenses/GetExpenses` | `Accountant.DashboardManagement.View` |
| F5 | `list_purchase_invoices` | P2 | R | `PurchaseInvoices/GetPurchaseInvoices`, `GetPurchaseInvoiceById` | `Warehouse.ReceiptManagement.View` |
| F6 | `list_finance_contracts` | P2 | R | `FinanceContracts/GetFinanceContractsList`, `GetFinanceContractDetail` | `Admin.FinanceContractManagement.View` |
| F7 | `list_supplier_contracts` | P2 | R | `SupplierContracts/GetSupplierContractsList`, `GetSupplierContractStatistics` | `Accountant.SupplierContractManagement.View` |
| F8 | `get_debt_logs_missing_proofs` | P3 | R | `DebtPayments/GetDebtLogsMissingProofs` | `Accountant.DebtPaymentManagement.View` |
| F9 | `record_expense` | **P4** | **W** | `Expenses/Commands` | *(xác nhận permission Create tương ứng module Expense khi implement)* |
| F10 | `confirm_debt_payment` | **P4** | **W** | `DebtPayments/Commands` | `Accountant.DebtPaymentManagement.Create` |

> ⚠️ **F1 (P&L) là dữ liệu nhạy cảm nhất hệ thống.** Cần permission chặt, và cân nhắc yêu cầu
> permission riêng `ManagerChat.Finance` ngoài permission gốc.

> ⚠️ **F10 (`confirm_debt_payment`)** ghi nhận tiền thật đã trả — bắt buộc **idempotency key** (Stage
> 17.9) để tránh double-submit khi mạng chập chờn, và bắt buộc kèm ảnh chứng từ (liên kết với F8
> `get_debt_logs_missing_proofs` — mục tiêu của F9/F10 là giảm số dòng F8 trả về theo thời gian).

### G. Nhân sự — module `hr`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| G1 | `list_employees` | P2 | R | `HR/GetEmployees`, `GetEmployeeById` | `Admin.EmployeeManagement.View` / `Accountant.EmployeeManagement.View` |
| G2 | `get_employee_kpi` | P2 | R | `HR/GetEmployeeKPIs` | như trên |
| G3 | `get_staff_performance` | P2 | R | `Statistical/GetStaffPerformance` | `Admin.DashboardManagement.View` |
| G4 | `get_payroll_summary` | P3 | R | `HR/GetPayrollSummary` | `Admin.PayrollManagement.View` / `Accountant.PayrollManagement.View` |
| G5 | `get_commission_records` | P3 | R | `HR/GetCommissionRecords`, `GetCommissionPolicies` | `Admin.PayrollManagement.View` |

> ⚠️ **G4, G5 = lương và hoa hồng.** Rất nhạy cảm. Cân nhắc **không đưa vào chatbot** ở giai đoạn đầu.
> Nếu đưa, phải là mức redaction `Minimal` (Stage 11.2) và có audit log riêng.

### H. Vận chuyển & Logistics — module `logistics`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| H1 | `get_shipment_tracking` | P1 | R | `Logistics/GetShipmentTracking` | `Order.OrderManagement.View` |
| H2 | `get_active_shipments` | P2 | R | `Logistics/GetActiveShipments` | như trên |
| H3 | `get_logistics_dashboard` | P2 | R | `Logistics/GetLogisticsDashboard` | như trên |
| H4 | `get_fulfillment_orders` | P2 | R | `Logistics/GetFulfillmentOrders`, `GetFulfillmentDetail` | như trên |
| H5 | `calculate_shipping_fee` | P2 | R | `Logistics/CalculateShippingFee` | như trên |
| H6 | `update_shipment_status` | **P4** | **W** | `Logistics/Commands` (đánh dấu đã giao/hoàn hàng) | `Order.OrderManagement.Update` |

### I. Nhà cung cấp — module `inventory`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| I1 | `search_suppliers` | P2 | R | `Suppliers/GetSuppliersList`, `GetSupplierById` | `Warehouse.SupplierManagement.View` |
| I2 | `get_supplier_statistics` | P2 | R | `Suppliers/GetSupplierStatistics` | như trên |

### J. Quản trị & Hệ thống — module `admin`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| J1 | `get_dashboard_overview` | P1 | R | `Statistical/GetAdminDashboardOverview`, `GetDashboardSummary` | `Admin.DashboardManagement.View` |
| J2 | `get_warehouse_report` | P2 | R | `Statistical/GetAdminWarehouseReport` | `Admin.DashboardManagement.View` |
| J3 | `get_revenue_analysis` | P2 | R | `Statistical/GetAdminRevenueAnalysis` | như trên |
| J4 | `get_store_settings` | P3 | R | `Settings/GetStoreSettings` | `Admin.SettingManagement.View` |
| J5 | `list_users_and_roles` | P3 | R | `Users`, `Permissions/GetAllPermissions` | `Admin.UserManagement.View` |
| J6 | `update_store_setting` | **P4** | **W** | `Settings/Commands` | `Admin.SettingManagement.Update` |

> ⚠️ **J5 lộ cấu trúc phân quyền** — hữu ích cho kẻ tấn công. Cân nhắc loại khỏi chatbot.
> ⚠️ **J6** chỉ cho phép sửa 3 khóa đã công khai qua `get_store_settings` (`DepositRatio`,
> `InventoryAlertLevel`, `OrderValueExceeds`) — whitelist cứng ở Command .NET, **không** expose
> setting nào khác qua chat dù về sau `SettingKeys` có thêm khóa mới.

### K. Tri thức nội bộ — module `knowledge`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| K1 | `search_knowledge` | P2 | R | Qdrant `knowledge_base` (Stage 12) | không yêu cầu (tài liệu chung) |
| K2 | `get_conversion_tools` | P3 | R | `ConversionTools/GetConversionTools` | — |

---

## 15.4. Tổng hợp theo đợt

| Đợt | Số tool | Nội dung | Ước lượng | Trạng thái thực thi |
|---|---|---|---|---|
| **P1** | **20** | Câu hỏi hằng ngày: sản phẩm, tồn kho, đơn hàng, doanh thu, khách hàng, sửa chữa, bảo hành, vận chuyển | 5–6 ngày | ✅ Code xong, build+test xanh |
| **P2** | **43** | Phủ rộng: kho chi tiết, nhà cung cấp, hợp đồng, marketing, tài chính, nhân sự, logistics, RAG | 6–8 ngày | ✅ 41/43 xong (A3, K1 chặn bởi Qdrant/Stage 12) |
| **P3** | **8** | Nhạy cảm (lương, hoa hồng, hệ thống) + tool ghi đầu tiên | 2–3 ngày | ✅ Code xong, build+test xanh |
| **P4** | **12** | **Mở rộng tool ghi (bổ sung 2026-07-31):** update trạng thái đơn/PR/phiếu sửa chữa, tạo lead/booking/warranty claim/voucher/expense, xác nhận thanh toán công nợ, sửa cấu hình cửa hàng | 4–6 ngày | ⏳ Mới ở mức đặc tả, chưa viết code |
| **P5** | **8** | **Mở rộng thêm (bổ sung 2026-07-31, đợt 2):** chỉnh sửa khách hàng/lead/sản phẩm/lịch hẹn, **xoá mềm + khôi phục** (voucher, tin tức) — KHÔNG có hard delete | 3–4 ngày | ⏳ Mới ở mức đặc tả, chưa viết code |
| **Tổng** | **91** | | | **69/91** đã có code (2 chặn ngoại lệ, 20 tool P4+P5 chưa làm) |

Chi tiết P1 theo nhóm: A(2) B(3) C(5) D(2) E(4) F(2) H(1) J(1) = **20**.
P3 gồm: B9, D7, F8, G4, G5, J4, J5, K2 = **8**.
P4 gồm: B10, C11, C12, D8, D9, E12, E13, E14, F9, F10, H6, J6 = **12**.
P5 gồm: A8, D10, D11, D12, D13, C13, C14, E15 = **8**.

**Không làm P2 trước khi P1 đạt tiêu chí chất lượng ở [13-STAGE-GUARDRAILS.md](13-STAGE-GUARDRAILS.md).**
Thêm tool khi độ chính xác chọn tool còn thấp chỉ làm mọi thứ tệ hơn. Nguyên tắc này áp dụng
**tương tự cho P4/P5** — không bật tool ghi mới nào cho người dùng thật trước khi P1+P2+P3 đã qua
eval đọc dữ liệu ổn định, vì tool ghi sai lựa chọn gây hậu quả nặng hơn tool đọc sai.

## 15.8. Đặc tả 12 tool ghi bổ sung (P4)

> **Trạng thái: đặc tả (spec) — CHƯA viết code.** Mục này định nghĩa `description` theo đúng
> template `DÙNG KHI / KHÔNG DÙNG KHI` (15.5 bước 4) để người triển khai sau dùng trực tiếp, tránh
> phải tự nghĩ lại. Khi implement, làm đúng khuôn 9 bước ở 16.11 như 69 tool đã có — không có gì
> khác biệt về hạ tầng, chỉ khác ở mức độ rủi ro nghiệp vụ nên DoD bổ sung 3 gate riêng (xem cuối mục).

| ID | Tool | Mô tả (`DÙNG KHI` / `KHÔNG DÙNG KHI`) |
|---|---|---|
| B10 | `update_purchase_request_status` | DÙNG KHI người dùng đã xác nhận rõ ràng muốn duyệt hoặc từ chối một yêu cầu mua hàng cụ thể theo ID (ví dụ: "duyệt PR 12 đi", sau khi đã xem chi tiết qua `get_purchase_request_detail` và xác nhận đúng). KHÔNG DÙNG KHI chỉ đang hỏi thông tin PR (dùng `list_purchase_requests`/`get_purchase_request_detail`) hoặc trạng thái đích không thuộc whitelist an toàn. |
| C11 | `update_order_status` | DÙNG KHI người dùng đã xác nhận muốn chuyển trạng thái một đơn hàng cụ thể sang trạng thái hợp lệ (xác nhận đơn, hủy đơn khi chưa xuất kho, đánh dấu đã giao). KHÔNG DÙNG KHI chỉ hỏi trạng thái hiện tại (dùng `get_order_status`) hoặc muốn đổi sang trạng thái không nằm trong whitelist nghiệp vụ. |
| C12 | `create_voucher` | DÙNG KHI người dùng (có thẩm quyền marketing) đã xác nhận đầy đủ thông tin voucher mới (mã, % giảm hoặc số tiền giảm, hạn dùng, số lượt dùng tối đa) và xác nhận tạo. KHÔNG DÙNG KHI thiếu bất kỳ thông tin bắt buộc nào, hoặc giá trị giảm giá vượt ngưỡng an toàn đã cấu hình (tool phải tự chặn, không dựa vào AI tự kiểm). |
| D8 | `create_lead` | DÙNG KHI người dùng muốn ghi nhận một lead mới vào hệ thống (khách hỏi qua chat, để lại thông tin liên hệ) và đã có tối thiểu tên + số điện thoại. KHÔNG DÙNG KHI chỉ đang tìm lead đã có (dùng `get_lead_pipeline`/`get_lead_detail`) hoặc thiếu thông tin liên hệ tối thiểu. |
| D9 | `update_lead_status` | DÙNG KHI người dùng xác nhận muốn chuyển một lead cụ thể (theo ID) sang giai đoạn pipeline khác (ví dụ "chuyển lead 8 sang Đang lái thử"). KHÔNG DÙNG KHI chỉ hỏi lead đang ở giai đoạn nào (dùng `get_lead_detail`) hoặc giai đoạn đích không hợp lệ theo thứ tự pipeline. |
| E12 | `create_booking_appointment` | DÙNG KHI người dùng (khách hoặc nhân viên thay khách) muốn đặt một lịch hẹn dịch vụ mới và đã xác nhận đủ thời gian/showroom/dịch vụ mong muốn. KHÔNG DÙNG KHI chỉ hỏi lịch hẹn đã có (dùng `list_booking_appointments`) hoặc slot thời gian đã được người khác đặt (tool phải tự kiểm tra trùng lịch, không tin tưởng AI). |
| E13 | `update_repair_order_status` | DÙNG KHI người dùng xác nhận muốn chuyển trạng thái một phiếu sửa chữa cụ thể sang bước tiếp theo hợp lệ trong workflow xưởng (ví dụ "đánh dấu phiếu 20 đã hoàn tất"). KHÔNG DÙNG KHI chỉ hỏi trạng thái hiện tại (dùng `get_repair_order_detail`) hoặc bước chuyển không đúng thứ tự workflow. |
| E14 | `create_warranty_claim` | DÙNG KHI người dùng đã xác nhận muốn ghi nhận một yêu cầu bảo hành mới cho một xe/sản phẩm cụ thể, đã có mô tả lỗi tối thiểu. KHÔNG DÙNG KHI chỉ hỏi điều khoản bảo hành chung (dùng `get_warranty_terms`) hoặc yêu cầu đã tồn tại (kiểm tra qua `list_warranty_claims` trước khi tạo trùng). |
| F9 | `record_expense` | DÙNG KHI người dùng (kế toán/quản lý) đã xác nhận đầy đủ thông tin một khoản chi phí mới (tên, số tiền, ngày chi, loại chi phí) và xác nhận ghi nhận. KHÔNG DÙNG KHI thiếu số tiền/loại chi phí, hoặc chỉ đang hỏi chi phí đã ghi (dùng `list_expenses`). |
| F10 | `confirm_debt_payment` | DÙNG KHI người dùng xác nhận đã thanh toán một khoản công nợ nhà cung cấp cụ thể và có/sẽ nộp ảnh chứng từ. KHÔNG DÙNG KHI chưa có chứng từ đính kèm (tool bắt buộc yêu cầu ảnh trước khi ghi, không ghi "tạm" rồi bổ sung sau) hoặc chỉ đang hỏi công nợ hiện tại (dùng `get_suppliers_with_debt`/`get_supplier_debt_detail`). |
| H6 | `update_shipment_status` | DÙNG KHI người dùng xác nhận muốn đánh dấu một vận đơn cụ thể đã giao xong hoặc bị hoàn hàng, khớp với thực tế đã xảy ra. KHÔNG DÙNG KHI chỉ hỏi tình trạng vận chuyển hiện tại (dùng `get_shipment_tracking`/`get_fulfillment_orders`) hoặc thông tin chưa được xác nhận từ đơn vị vận chuyển. |
| J6 | `update_store_setting` | DÙNG KHI người dùng (Admin) đã xác nhận muốn đổi một trong 3 tham số vận hành công khai (`DepositRatio`, `InventoryAlertLevel`, `OrderValueExceeds`) sang giá trị cụ thể trong khoảng hợp lệ. KHÔNG DÙNG KHI muốn đổi setting khác không nằm trong whitelist, hoặc giá trị nằm ngoài khoảng an toàn đã cấu hình ở Command .NET. |

### Gate bổ sung cho P4 (ngoài khuôn 9 bước chung ở 16.11)

1. **Whitelist trạng thái/giá trị đích ở tầng Command .NET, không ở prompt.** Mọi tool "update_*"
   (B10, C11, E13, H6, J6) phải chặn giá trị đích không hợp lệ ngay trong Command Handler — nếu AI
   bị dụ prompt injection truyền giá trị lạ, Command vẫn từ chối.
2. **Idempotency key bắt buộc cho tool tạo bản ghi tài chính** (C12, F9, F10) — theo đúng cơ chế run
   token của Stage 17.9, tránh double-submit khi retry.
3. **Audit log riêng, không dùng chung log tool đọc** — mọi lệnh gọi P4 phải ghi rõ: user thật (không
   phải "AI"), tool, tham số, kết quả, thời điểm — để truy vết khi có tranh chấp nghiệp vụ.

## 15.9. Đặc tả 8 tool bổ sung (P5) — Chỉnh sửa mở rộng + Xoá mềm & Khôi phục

> **Trạng thái: đặc tả (spec) — CHƯA viết code.**
>
> **Quyết định đã chốt 2026-07-31 (trả lời câu hỏi "có thêm tool xoá được không"):**
> **KHÔNG có tool xoá vĩnh viễn (hard delete) nào được thêm vào chatbot** — đây là nguyên tắc bất
> biến giữ nguyên từ bản gốc (15.7, DoD P3), áp dụng cho cả P5. Lý do: AI có thể bị prompt injection,
> hiểu nhầm ý người dùng, hoặc người dùng xác nhận nhầm khi thao tác nhanh qua chat — với tool đọc
> sai thì sửa lại câu trả lời, nhưng **xoá vĩnh viễn sai thì mất dữ liệu không thể khôi phục.**
>
> Thay vào đó, "xoá" ở P5 nghĩa là **soft-delete** — dùng đúng cơ chế `DeletedAt`/`IsActive` đã có
> sẵn trong toàn bộ codebase (`DataFetchMode.ActiveOnly/DeletedOnly/All`), và **mỗi tool xoá mềm bắt
> buộc phải có tool khôi phục đối xứng đi kèm** — không có ngoại lệ. Nếu sau này thêm xoá mềm cho
> entity khác, luôn thêm theo cặp (deactivate_X + restore_X), không thêm lẻ.

| ID | Tool | Loại | Mô tả (`DÙNG KHI` / `KHÔNG DÙNG KHI`) |
|---|---|---|---|
| A8 | `update_product_info` | Sửa | DÙNG KHI người dùng (thủ kho/quản lý) đã xác nhận muốn đổi giá bán hoặc thông tin cơ bản (tên, mô tả) của một sản phẩm cụ thể theo ID. KHÔNG DÙNG KHI chỉ hỏi giá hiện tại (dùng `get_product_detail`/`get_product_price_list`) hoặc giá mới nằm ngoài khoảng hợp lý (chênh lệch bất thường so với giá cũ — Command phải tự cảnh báo/chặn, không chỉ dựa vào AI). |
| D10 | `update_customer_profile` | Sửa | DÙNG KHI người dùng xác nhận muốn sửa thông tin liên hệ (SĐT, địa chỉ) hoặc ghi chú của một khách hàng cụ thể theo ID, và đã đọc lại thông tin mới cho người dùng xác nhận (vì đây là PII). KHÔNG DÙNG KHI chỉ đang tra cứu thông tin (dùng `get_customer_profile`) hoặc thông tin thay đổi chưa được xác nhận lại rõ ràng. |
| D11 | `update_lead_info` | Sửa | DÙNG KHI người dùng xác nhận muốn sửa thông tin liên hệ/xe quan tâm/ghi chú của một lead cụ thể theo ID — khác `update_lead_status` (chỉ đổi giai đoạn pipeline, không đổi thông tin). KHÔNG DÙNG KHI chỉ muốn chuyển giai đoạn pipeline (dùng `update_lead_status`) hoặc chỉ đang tra cứu (dùng `get_lead_detail`). |
| E15 | `update_booking_appointment` | Sửa | DÙNG KHI người dùng xác nhận muốn đổi giờ hẹn (`action="reschedule"`) hoặc huỷ (`action="cancel"`) một lịch hẹn dịch vụ cụ thể theo ID. KHÔNG DÙNG KHI slot giờ mới đã có người đặt (Command phải tự kiểm tra trùng lịch trước khi ghi) hoặc chỉ đang tra cứu lịch hẹn (dùng `list_booking_appointments`). |
| C13 | `deactivate_voucher` | Xoá mềm | DÙNG KHI người dùng (marketing) xác nhận muốn ngừng hiệu lực một voucher cụ thể theo mã/ID trước hạn (ví dụ chương trình kết thúc sớm). KHÔNG DÙNG KHI voucher đã hết hạn tự nhiên (không cần thao tác) hoặc chỉ đang hỏi danh sách voucher (dùng `list_vouchers`). Đây là **soft-delete** — voucher vẫn còn trong hệ thống, khôi phục được qua `restore_voucher`. |
| C14 | `restore_voucher` | Khôi phục | DÙNG KHI người dùng xác nhận muốn kích hoạt lại một voucher đã bị `deactivate_voucher` trước đó (ví dụ tắt nhầm, hoặc quyết định gia hạn chương trình). KHÔNG DÙNG KHI voucher chưa từng bị vô hiệu hoá, hoặc đã hết hạn thật sự theo `EndDate` (khôi phục không tự động gia hạn ngày). |
| D12 | `deactivate_news` | Xoá mềm | DÙNG KHI người dùng (marketing) xác nhận muốn gỡ một bài viết tin tức cụ thể khỏi hiển thị công khai. KHÔNG DÙNG KHI chỉ muốn sửa nội dung bài viết (chưa có tool `update_news` ở đợt này — nếu cần, bổ sung ở đợt sau) hoặc chỉ đang tra cứu (dùng `list_news`). Đây là **soft-delete**, khôi phục được qua `restore_news`. |
| D13 | `restore_news` | Khôi phục | DÙNG KHI người dùng xác nhận muốn hiển thị lại một bài viết tin tức đã bị `deactivate_news` trước đó. KHÔNG DÙNG KHI bài viết chưa từng bị gỡ. |

### Gate bổ sung cho P5 (ngoài khuôn 9 bước chung ở 16.11 và 3 gate của P4)

1. **Mọi tool "xoá" phải có tool "khôi phục" đối xứng, merge cùng một PR — không merge lẻ.**
   Test chặn: viết 1 test `test_moi_tool_xoa_deu_co_tool_khoi_phuc` kiểm tra theo quy ước đặt tên
   (`deactivate_X` ⇔ `restore_X`) ngay trong `test_tool_registry.py`, tương tự các test trần đã có.
2. **Xoá mềm không bao giờ động tới bản ghi liên quan đã hoàn tất** — ví dụ `deactivate_voucher`
   không được ảnh hưởng đến các đơn hàng đã áp dụng voucher đó trong quá khứ.
3. **D10/D11 sửa PII trực tiếp** — bắt buộc đọc lại giá trị mới cho người dùng nghe/xem trước khi
   ghi (không chỉ 1 câu "ok xác nhận" chung chung), đúng tinh thần Stage 11 redaction.
4. **E15 kiểm tra trùng lịch ở tầng Command**, không tin AI đã tự kiểm tra qua `list_booking_appointments`
   trước đó — dữ liệu có thể đã đổi giữa 2 lượt gọi.

---

## 15.5. Khuôn mẫu triển khai một tool

Để 71 tool không thành 71 kiểu code khác nhau, mỗi tool đi theo đúng khuôn mẫu sau:

### Bước 1 — Query .NET (tái sử dụng handler sẵn có)
```
Application/Features/ChatTools/Queries/GetStockOnHandForChat/
  GetStockOnHandForChatQuery.cs
  GetStockOnHandForChatQueryHandler.cs
  ChatStockDto.cs
```
Handler **gọi lại** repository/query có sẵn, chỉ đổi hình dạng DTO cho gọn. Không viết lại logic nghiệp vụ.

### Bước 2 — Endpoint
```csharp
[HttpPost("inventory/stock-on-hand")]
[RequirePermission(Permissions.Warehouse.InventoryReportManagement.View)]
public async Task<IActionResult> GetStockOnHand(
    [FromBody] GetStockOnHandForChatRequest request, CancellationToken ct)
    => Ok(await sender.Send(new GetStockOnHandForChatQuery(request.ProductId, request.Limit), ct));
```
> Kiểm tra tên attribute thật trong `WebAPI/Attributes/` trước khi viết.

### Bước 3 — Khai báo ToolSpec (sidecar)
```python
ToolSpec(
    name="get_stock_on_hand",
    module="inventory",
    required_permissions=("Permissions.Warehouse.InventoryReportManagement.View",),
    is_write=False,
    factory=make_stock_tool,
)
```

### Bước 4 — Mô tả theo template Stage 13.9
`Làm gì · DÙNG KHI · KHÔNG DÙNG KHI · TRẢ VỀ`

### Bước 5 — Summarizer + nhãn tiếng Việt
```python
SUMMARIZERS["get_stock_on_hand"] = lambda r: f"{r['totalCount']} biến thể, tổng tồn {r['totalQty']}"
```
```ts
TOOL_LABELS.get_stock_on_hand = "Kiểm tra tồn kho";
```

**Checklist 5 bước này phải đủ mới được merge.** Thiếu bước 5 → Stage 11 hiển thị "Đã hoàn tất"
vô nghĩa; thiếu bước 4 → agent chọn sai.

> ⚠️ **Khuôn mẫu này đã được mở rộng thành 9 bước** ở
> [16-STAGE-TOOL-DATA-FIDELITY.md](16-STAGE-TOOL-DATA-FIDELITY.md) mục 16.11 — bổ sung
> envelope, parity test, contract test và cờ shadow. **Dùng bản 9 bước**, 5 bước ở đây chỉ là
> phần khung. Làm Stage 16 **trước** khi bắt đầu đợt P1, nếu không sẽ phải sửa lại cả 71 tool.

---

## 15.6. Ma trận permission → tool khả dụng

Kiểm chứng trước khi code — mỗi vai trò phải thấy số tool hợp lý (Stage 13.2):

| Vai trò điển hình | Module có quyền | Số tool ước tính |
|---|---|---|
| Admin | tất cả | ~70 sau lọc quyền → **≤ 20 sau trần request** (13.3b) |
| Kế toán | Accountant | ~17 (tính cả F9, F10) |
| Thủ kho | Warehouse | ~15 (tính cả B10) |
| Nhân viên bán hàng | Order | ~14 (tính cả C11) |
| Kỹ thuật viên xưởng | Factory | ~14 (tính cả E12, E13, E14) |
| Marketing | Marketing | ~11 (tính cả C12, D8, D9) |

> Admin có quyền với cả 91 tool — đó là lý do router 2 tầng (13.3) và trần cứng (13.3b)
> **không phải tuỳ chọn**. Lọc theo quyền một mình không giới hạn được gì cho vai trò Admin.

---

## 15.7. Rủi ro riêng của Stage này

| Rủi ro | Giảm thiểu |
|---|---|
| 91 tool → agent chọn sai nhiều | Router 2 tầng + **trần 20 tool/request** (13.3b) + eval sau mỗi đợt |
| Tool ghi (P4/P5) bị gọi nhầm/bị dụ qua prompt injection | Whitelist giá trị đích ở Command .NET (không ở prompt) + Plan Mode + confirm bắt buộc — xem gate P4 cuối 15.8, gate P5 cuối 15.9 |
| Xoá mềm (P5) bị dùng như xoá thật, không ai nhớ khôi phục | Mọi `deactivate_X` bắt buộc có `restore_X` đối xứng + test chặn ở CI (xem gate P5 #1) |
| `customer`/`service`/`inventory` hết chỗ mở rộng | Đã ghi rõ ở 15.2 — module mới cần tách nhỏ hơn, không nới trần |
| Thêm tool mới làm module vượt trần | `test_tool_registry.py` chặn ở CI (13.3c) — đã từng xảy ra ở bản nháp |
| Tool trùng chức năng (`list_orders` vs `get_order_statistics`) | Mô tả "KHÔNG DÙNG KHI" nêu tên tool đúng |
| DTO phình to → tốn token | Trần 10 bản ghi, chỉ field cần thiết |
| Permission ánh xạ sai → 403 hàng loạt | Test ma trận 15.6 cho từng vai trò |
| Tool nhạy cảm (lương, P&L) lọt vào tay sai | Rà soát thủ công danh sách P3 trước khi bật |
| Code lặp 60 lần | Khuôn mẫu 15.5 + generic base class |
| Nhóm feature mới thêm sau này không có tool | Ghi vào `CLAUDE.md` / `RULES.md`: feature mới phải cân nhắc tool |

---

## Definition of Done — Stage 15

### Sau đợt P1
- [ ] 20 tool P1 hoạt động E2E, đủ 9 bước của khuôn mẫu (15.5 + 16.11).
- [ ] Ma trận permission 15.6 kiểm chứng đúng cho cả 6 vai trò.
- [ ] **Không module nào vượt 10 tool; mọi cặp 2 module ≤ 20** — `test_tool_registry.py` xanh.
- [ ] **Vai trò Admin: số tool nạp vào một request ≤ 20**, không phải 60.
- [ ] Bộ eval mở rộng lên ≥ 60 câu hỏi, độ chính xác chọn tool ≥ 90%.
- [ ] Router 2 tầng hoạt động với đủ 6 module + `knowledge` + `none`.
- [ ] Trung vị số tool call/run ≤ 2 (Stage 14).

### Sau đợt P2
- [ ] 63 tool hoạt động (P1+P2), độ chính xác chọn tool vẫn ≥ 88%.
- [ ] Tool trả PII (D1, D2) đã qua redaction, kiểm chứng bằng DevTools + query DB.
- [ ] Bộ eval ≥ 120 câu hỏi phủ cả 6 module.

### Sau đợt P3
- [ ] Danh sách tool nhạy cảm được rà soát thủ công và ký duyệt.
- [ ] Tool ghi đầu tiên (B9) chạy qua Plan Mode + confirm + audit log + idempotency key.
- [ ] Không có tool xoá vĩnh viễn nào được cấp cho AI.
- [ ] `RULES.md` / `CLAUDE.md` bổ sung quy định: feature mới cần đánh giá có cần tool không.

### Sau đợt P4 (12 tool ghi bổ sung — 15.8)
- [ ] Cả 12 tool đủ khuôn 9 bước (16.11) **+ 3 gate riêng của P4** (whitelist ở Command .NET,
      idempotency key cho C12/F9/F10, audit log riêng — xem cuối mục 15.8).
- [ ] Mỗi tool "update_*"/"create_*" có test chặn giá trị đích/tham số không hợp lệ ở tầng Command,
      không chỉ dựa vào `description` để "khuyên" AI đừng gọi sai.
- [ ] **Không module nào vượt 10 tool sau khi thêm** (`inventory` chạm đúng 10 — chặn thêm tool mới
      vào module này) — `test_tool_registry.py` xanh lại sau khi thêm cả 12.
- [ ] Diễn tập sự cố: giả lập AI gọi sai tool ghi (ví dụ update sai trạng thái đơn) → xác nhận audit
      log đủ thông tin để revert thủ công, và Plan Mode thực sự chặn được nếu chưa xác nhận.
- [ ] F10 (`confirm_debt_payment`) test riêng: từ chối ghi nhận khi thiếu ảnh chứng từ.
- [ ] Không tool ghi nào bật `full` ngay — tất cả bắt đầu ở cờ `shadow`, tối thiểu 5 ngày quan sát
      trước khi lên `canary`, đúng tinh thần bước 9 của khuôn mẫu.

### Sau đợt P5 (8 tool bổ sung — chỉnh sửa mở rộng + xoá mềm/khôi phục, 15.9)
- [ ] Cả 8 tool đủ khuôn 9 bước (16.11) **+ 4 gate riêng của P5** (xem cuối mục 15.9).
- [ ] **Mọi tool `deactivate_X` có đúng 1 tool `restore_X` đối xứng** — test
      `test_moi_tool_xoa_deu_co_tool_khoi_phuc` xanh, chặn ở CI như các test trần khác.
- [ ] Xác nhận lại bằng tay: `deactivate_voucher`/`deactivate_news` chỉ set `DeletedAt`/`IsActive`,
      **không** có đường nào trong Command dẫn tới xoá bản ghi thật khỏi DB.
- [ ] `customer`, `service`, `inventory` xác nhận đúng 10/10 tool — `test_khong_module_nao_vuot_tran`
      và `test_hai_module_bat_ky_khong_vuot_tran_request` xanh với catalog 91 tool.
- [ ] D10/D11 (sửa PII) đã qua redaction Stage 11, kiểm chứng bằng DevTools + query DB.
- [ ] Diễn tập: gọi `deactivate_voucher` → xác nhận voucher biến mất khỏi `list_vouchers` nhưng vẫn
      còn trong DB (`DeletedAt` có giá trị) → gọi `restore_voucher` → xác nhận xuất hiện lại đúng
      dữ liệu cũ, không mất field nào.
