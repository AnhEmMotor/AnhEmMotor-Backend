# Tài liệu Kiểm thử tính năng Tách luồng Đơn hàng (SalesOrder Split Endpoints)

Tài liệu này mô tả chi tiết các bài kiểm thử (Unit, Controller, và Integration Tests) đã được viết, sửa đổi và bổ sung để đảm bảo tính chính xác và an toàn bảo mật cho tính năng tách luồng danh sách đơn hàng.

---

## 1. Tổng quan Thay đổi Tính năng

Để tối ưu hóa bảo mật và phân quyền chi tiết, endpoint lấy tất cả đơn hàng không qua bộ lọc (`GET /api/v1/SalesOrders` với quyền `Outputs.View`) đã được loại bỏ hoàn toàn. Thay vào đó, hệ thống hỗ trợ 2 endpoint riêng biệt:
1. **GET /api/v1/SalesOrders/confirmed**: Lấy danh sách các phiếu bán hàng đã xác nhận. Yêu cầu quyền `Outputs.ViewConfirmed`.
2. **GET /api/v1/SalesOrders/unconfirmed**: Lấy danh sách các phiếu tạm chưa xác nhận. Yêu cầu quyền `Outputs.ViewUnconfirmed`.

---

## 2. Danh sách các File Kiểm thử Liên quan

Các thay đổi, sửa đổi và thêm mới tập trung vào các file kiểm thử sau:
- **[New/Modified]** `ControllerTests/SalesOrderSplitEndpoints.cs` ([SalesOrderSplitEndpoints.cs](file:///c:/AnhEmMoto/AnhEmMotor-Backend/ControllerTests/SalesOrderSplitEndpoints.cs)): Kiểm thử tầng Controller cho các endpoint tách mới.
- **[Modified]** `ControllerTests/SalesOrder.cs` ([SalesOrder.cs](file:///c:/AnhEmMoto/AnhEmMotor-Backend/ControllerTests/SalesOrder.cs)): Loại bỏ các test case của endpoint cũ đã bị xóa.
- **[Modified]** `UnitTests/SalesOrder.cs` ([SalesOrder.cs](file:///c:/AnhEmMoto/AnhEmMotor-Backend/UnitTests/SalesOrder.cs)): Kiểm thử tầng Application (Query & Domain) cho việc gom nhóm trạng thái và lọc.
- **[Modified]** `IntegrationTests/SalesOrder.cs` ([SalesOrder.cs](file:///c:/AnhEmMoto/AnhEmMotor-Backend/IntegrationTests/SalesOrder.cs)): Sửa đổi các test tích hợp để tương thích với endpoint và phân quyền mới.

---

## 3. Chi tiết các Test Case (Mới & Đã Sửa)

### A. Tầng Controller (ControllerTests)
Tập tin `ControllerTests/SalesOrderSplitEndpoints.cs` kiểm tra tính chính xác của định tuyến (Routing), phân quyền (Authorization) và hành vi điều hướng dữ liệu qua Mediator.

| ID Test | Tên Test Case | Mô tả chi tiết mục đích kiểm thử |
| :--- | :--- | :--- |
| **SO_117** | `SalesOrdersController_ShouldExposeSplitListEndpointsWithNewPermissions` | Đảm bảo rằng Controller cung cấp đúng 2 endpoint `confirmed` và `unconfirmed` tương ứng với các quyền `Outputs.ViewConfirmed` và `Outputs.ViewUnconfirmed`. |
| **SO_118** | `GetConfirmedOutputs_WithSieveModel_ReturnsConfirmedOutputs` | Xác nhận hành vi của endpoint `confirmed` gửi đúng truy vấn `GetOutputsListQuery` chứa danh sách các mã trạng thái đã xác nhận (`OrderStatus.ConfirmedOrderStatuses`) sang Mediator. |
| **SO_119** | `GetUnconfirmedOutputs_WithSieveModel_ReturnsUnconfirmedOutputs` | Xác nhận hành vi của endpoint `unconfirmed` gửi đúng truy vấn `GetOutputsListQuery` chứa danh sách các mã trạng thái chưa xác nhận (`OrderStatus.UnconfirmedOrderStatuses`) sang Mediator. |
| **SO_120** | `SalesOrdersController_ShouldNotExposeUnfilteredListEndpoint` | Kiểm thử bảo mật tĩnh (Static Analysis Security Test): Đảm bảo Controller tuyệt đối không expose bất kỳ endpoint HTTP GET gốc (`GET /api/v1/SalesOrders`) lấy toàn bộ danh sách đơn hàng mà không qua phân luồng. |
| **SO_121** | `GetConfirmedOutputs_WhenMediatorFails_ReturnsFailureResult` <br>*(Bổ sung mới)* | Đảm bảo xử lý lỗi chính xác ở endpoint `confirmed` khi Mediator trả về trạng thái thất bại (Failure), trả lại thông tin lỗi thích hợp. |
| **SO_122** | `GetUnconfirmedOutputs_WhenMediatorFails_ReturnsFailureResult` <br>*(Bổ sung mới)* | Đảm bảo xử lý lỗi chính xác ở endpoint `unconfirmed` khi Mediator trả về trạng thái thất bại (Failure), trả lại thông tin lỗi thích hợp. |

> [!NOTE]
> Các test case cũ kiểm thử endpoint không lọc tại `ControllerTests/SalesOrder.cs` (cụ thể là bài test **SO_083 - GetOutputs**) đã được **xóa bỏ** hoàn toàn vì endpoint đó không còn tồn tại trên Controller.

---

### B. Tầng Unit & Logic Nghiệp vụ (UnitTests)
Tập tin `UnitTests/SalesOrder.cs` kiểm thử logic tại Handler và các định nghĩa nhóm trạng thái ở tầng Domain.

| ID Test | Tên Test Case | Mô tả chi tiết mục đích kiểm thử |
| :--- | :--- | :--- |
| **SO_115** | `GetOutputsList_WithStatusIds_ShouldApplyRepositoryFilter` | Đảm bảo `GetOutputsListQueryHandler` áp dụng đúng bộ lọc SQL / Repository dựa trên tham số danh sách `StatusIds` được truyền từ Query. |
| **SO_116** | `OrderStatus_ShouldExposeConfirmedAndUnconfirmedStatusGroups` | Kiểm chứng xem lớp hằng số `OrderStatus` ở tầng Domain có khai báo chuẩn các nhóm trạng thái đã xác nhận (bao gồm `ConfirmedCod`, `Completed`, v.v.) và chưa xác nhận (bao gồm `Pending`, `WaitingDeposit`, v.v.) hay không. |

---

### C. Tầng Tích hợp (IntegrationTests)
Tập tin `IntegrationTests/SalesOrder.cs` kiểm thử đầu cuối (End-to-End API Integration). Các bài kiểm thử nghiệp vụ danh sách đơn hàng đã được cập nhật từ luồng lấy danh sách chung sang luồng phiếu tạm (unconfirmed) với quyền hạn mới.

| ID Test | Tên Test Case | Thay đổi & Mục đích tích hợp |
| :--- | :--- | :--- |
| **SO_067** | `GetOutputs_WithFilters_ReturnsFilteredResults` | Được cập nhật để gửi request tới `/api/v1/SalesOrders/unconfirmed` và xác thực tài khoản có quyền `Outputs.ViewUnconfirmed` thay vì `Outputs.View`. |
| **SO_069** | `GetOutputs_SortByCreatedAtDesc_ReturnsSortedResults` | Được cập nhật để kiểm thử sắp xếp trên endpoint mới `/api/v1/SalesOrders/unconfirmed` sử dụng token quyền `Outputs.ViewUnconfirmed`. |
| **SO_070** | `GetOutputs_WithPagination_ReturnsPagedResults` | Được cập nhật để kiểm thử phân trang trên endpoint mới `/api/v1/SalesOrders/unconfirmed` sử dụng token quyền `Outputs.ViewUnconfirmed`. |

---

## 4. Hướng dẫn Chạy Kiểm thử

### Chạy các Unit Tests và Controller Tests (Khuyên dùng khi chạy local)
Do các Integration Tests yêu cầu Docker Testcontainers (khởi chạy một PostgreSQL instance tạm thời), bạn có thể chạy nhanh các kiểm thử đơn vị độc lập và kiểm thử controller bằng cách sử dụng bộ lọc sau:

```bash
dotnet test --filter "FullyQualifiedName~ControllerTests|FullyQualifiedName~UnitTests"
```

### Chạy toàn bộ Tests (Yêu cầu Docker đang chạy)
Nếu máy bạn đã chạy Docker Desktop, bạn có thể thực hiện chạy toàn bộ hệ thống test bao gồm cả Integration Tests:

```bash
dotnet test
```
