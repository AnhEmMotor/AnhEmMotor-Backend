# Từ điển nghiệp vụ — Chatbot AI

> Stage 16.4. Mỗi tool tài chính/tổng hợp phải khai báo đúng 1 định nghĩa dưới đây trong trường
> `Definition` của `ChatToolEnvelope`. Đây là bản DRAFT do dev soạn — **cần người phụ trách nghiệp vụ
> xác nhận trước khi coi là chốt** (điền vào mục "Xác nhận bởi" của từng khái niệm).

## Doanh thu

- **Định nghĩa**: Tổng tiền hàng sau chiết khấu, chưa gồm phí vận chuyển.
- **Nguồn chuẩn**: `IStatisticalReadRepository.GetDailyRevenueAsync`
- **Đơn vị**: VND
- **Loại trừ**: Đơn huỷ, đơn nháp, bản ghi soft-delete (`DeletedAt != null`)
- **Xác nhận bởi**: _(chưa xác nhận — chờ người phụ trách nghiệp vụ)_

## Số đơn hàng

- **Định nghĩa**: Số đơn có trạng thái nằm trong tập trạng thái hợp lệ (xem `GetOrderStatusMap`).
- **Nguồn chuẩn**: `IOutputReadRepository.GetByIdWithDetailsAsync`
- **Đơn vị**: đơn
- **Loại trừ**: Đơn nháp (`DraftOrderManagement`), đơn huỷ
- **Xác nhận bởi**: _(chưa xác nhận)_

## Lợi nhuận

- **Định nghĩa**: Doanh thu − giá vốn − chi phí.
- **Nguồn chuẩn**: `Statistical/GetPnlReport`
- **Đơn vị**: VND
- **Loại trừ**: — (kế thừa loại trừ của Doanh thu)
- **Xác nhận bởi**: _(chưa xác nhận)_

## Tồn kho

- **Định nghĩa**: Số lượng khả dụng tại kho theo variant sản phẩm.
- **Nguồn chuẩn**: `IStatisticalReadRepository.GetProductStockAndPriceAsync` /
  `GetProductPerformanceTableAsync`
- **Đơn vị**: cái
- **Loại trừ**: Hàng đang giữ cho đơn chưa giao
- **Xác nhận bởi**: _(chưa xác nhận)_

## Khách hàng mới

- **Định nghĩa**: Khách có đơn hàng đầu tiên phát sinh trong kỳ đang xét.
- **Nguồn chuẩn**: `Customer/GetCustomerProfile360`
- **Đơn vị**: người
- **Loại trừ**: —
- **Xác nhận bởi**: _(chưa xác nhận)_

## "Tháng này" / "Hôm nay"

- **Định nghĩa**: Tính theo giờ Việt Nam (GMT+7, `Asia/Ho_Chi_Minh`) — "hôm nay" là ngày hiện tại theo
  `IServerDateProvider.VietnamToday`; "tháng này" là từ ngày 1 tháng hiện tại (giờ VN) đến hôm nay.
  **Không** được suy theo giờ UTC trần — lệch ngày trong khung 00:00–07:00 giờ VN (xem Stage 16.2 mục #2).
- **Nguồn chuẩn**: `IServerDateProvider` (`Application/Common/Interfaces/IServerDateProvider.cs`)
- **Đơn vị**: —
- **Loại trừ**: —
- **Xác nhận bởi**: _(chưa xác nhận)_
