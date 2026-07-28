# Stage 15 — Danh mục Tool đầy đủ cho toàn bộ dự án

> Yêu cầu bổ sung · Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 8–12 ngày (chia 3 đợt)
> Phụ thuộc: **Stage 3** (hạ tầng tool calling), **Stage 13 + 20** (guardrails & tool scoping —
> bắt buộc xong trước khi rollout P1), **Stage 16** (envelope & parity test)

Stage 3 làm 5–6 tool mẫu để chứng minh hạ tầng chạy. Stage này **quét toàn bộ dự án** và
lập danh mục tool phủ hết các phân hệ nghiệp vụ.

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

1. **Chỉ đọc trong đợt 1 và 2.** Tool ghi để đợt 3, và phải qua Plan Mode + confirm (Stage 13.5).
2. **Tool ánh xạ theo *câu hỏi*, không theo *endpoint*.** 57 feature × ~8 query = ~450 query,
   nhưng chỉ cần ~71 tool. Gộp theo ý định của người dùng.
3. **Mỗi tool khai báo đủ:** `required_permissions`, `is_write`, `module`, `summarizer` (Stage 11.3).
4. **Không tạo tool cho:** export file, upload, sitemap, template import, audit log, endpoint
   nội bộ của FE (`GetActiveVariantLiteListFor*`, `GetOrderStatusTransitionMap`...).
   Chúng phục vụ UI, không phải câu hỏi của người dùng.
5. **Giới hạn bản ghi:** mặc định 10, trần 25 (50 với danh sách nhân sự).
6. **Tối đa 10 tool/module**, và **mọi cặp 2 module ≤ 20 tool** (trần một request).
   Đây là **bất biến có test chặn**, không phải hướng dẫn —
   xem [13-STAGE-GUARDRAILS.md](13-STAGE-GUARDRAILS.md) mục 13.3b và 13.3c.

### Phân bổ module — đã tách lại để không vượt trần

Bản nháp đầu của danh mục có `service` = 11 tool và `inventory` = 11 tool, **vượt trần**.
Đã tách thành 13 module:

| Module | Nhóm tool | Số tool |
|---|---|---|
| `product` | A1–A6 | 6 |
| `supplier` | A7, I1, I2 | 3 |
| `inventory` | B1–B9 | 9 |
| `sales` | C1–C8 | 8 |
| `contract` | C9, F6, F7 | 3 |
| `customer` | D1–D6 | 6 |
| `marketing` | C10, D7 | 2 |
| `service` | E1, E2, E6–E9, E11 | 7 |
| `warranty` | E3–E5, E10 | 4 |
| `finance` | F1–F5, F8 | 6 |
| `hr` | G1–G5 | 5 |
| `logistics` | H1–H5 | 5 |
| `admin` | J1–J5 | 5 |
| `knowledge` | K1, K2 | 2 |
| **Tổng** | | **71** |

Mỗi tool thuộc **đúng một** module — `A7` (`get_supplier_prices_for_variant`) chuyển sang
`supplier` nên `product` còn 6, không phải 7.

Ba cặp lớn nhất: `inventory`+`sales` = **17**, `product`+`inventory` = 15,
`inventory`+`service` = 16. Tất cả **≤ 20** ✅

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

> **Gộp quan trọng:** `search_products` trả luôn **giá + tồn kho + biến thể**, tránh agent phải
> gọi thêm 2 tool (Stage 14.2b).

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

> **Gộp:** `get_sales_summary` nhận `compare_with_previous: bool` để trả cả kỳ trước trong 1 lần gọi.

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

> ⚠️ **D1, D2 trả PII** (tên, số điện thoại, địa chỉ). Bắt buộc chạy qua redaction của
> [11-STAGE-REASONING-TRANSPARENCY.md](11-STAGE-REASONING-TRANSPARENCY.md) — panel suy nghĩ **không**
> được hiện thông tin khách hàng thô ở Production.

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

> ⚠️ **F1 (P&L) là dữ liệu nhạy cảm nhất hệ thống.** Cần permission chặt, và cân nhắc yêu cầu
> permission riêng `ManagerChat.Finance` ngoài permission gốc.

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

> ⚠️ **J5 lộ cấu trúc phân quyền** — hữu ích cho kẻ tấn công. Cân nhắc loại khỏi chatbot.

### K. Tri thức nội bộ — module `knowledge`

| # | Tool | Đợt | R/W | Nguồn | Permission gốc |
|---|---|---|---|---|---|
| K1 | `search_knowledge` | P2 | R | Qdrant `knowledge_base` (Stage 12) | không yêu cầu (tài liệu chung) |
| K2 | `get_conversion_tools` | P3 | R | `ConversionTools/GetConversionTools` | — |

---

## 15.4. Tổng hợp theo đợt

| Đợt | Số tool | Nội dung | Ước lượng |
|---|---|---|---|
| **P1** | **20** | Câu hỏi hằng ngày: sản phẩm, tồn kho, đơn hàng, doanh thu, khách hàng, sửa chữa, bảo hành, vận chuyển | 5–6 ngày |
| **P2** | **43** | Phủ rộng: kho chi tiết, nhà cung cấp, hợp đồng, marketing, tài chính, nhân sự, logistics, RAG | 6–8 ngày |
| **P3** | **8** | Nhạy cảm (lương, hoa hồng, hệ thống) + tool ghi đầu tiên | 2–3 ngày |
| **Tổng** | **71** | | |

Chi tiết P1 theo nhóm: A(2) B(3) C(5) D(2) E(4) F(2) H(1) J(1) = **20**.
P3 gồm: B9, D7, F8, G4, G5, J4, J5, K2 = **8**.

**Không làm P2 trước khi P1 đạt tiêu chí chất lượng ở [13-STAGE-GUARDRAILS.md](13-STAGE-GUARDRAILS.md).**
Thêm tool khi độ chính xác chọn tool còn thấp chỉ làm mọi thứ tệ hơn.

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
| Admin | tất cả | ~60 sau lọc quyền → **≤ 20 sau trần request** (13.3b) |
| Kế toán | Accountant | ~14 |
| Thủ kho | Warehouse | ~13 |
| Nhân viên bán hàng | Order | ~12 |
| Kỹ thuật viên xưởng | Factory | ~11 |
| Marketing | Marketing | ~8 |

> Admin có quyền với cả 71 tool — đó là lý do router 2 tầng (13.3) và trần cứng (13.3b)
> **không phải tuỳ chọn**. Lọc theo quyền một mình không giới hạn được gì cho vai trò Admin.

---

## 15.7. Rủi ro riêng của Stage này

| Rủi ro | Giảm thiểu |
|---|---|
| 71 tool → agent chọn sai nhiều | Router 2 tầng + **trần 20 tool/request** (13.3b) + eval sau mỗi đợt |
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
