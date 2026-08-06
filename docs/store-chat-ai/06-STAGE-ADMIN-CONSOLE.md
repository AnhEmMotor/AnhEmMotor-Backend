# Stage 06 — Trang quản trị phiên chat Store

> Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 2–3 ngày · Phụ thuộc: **Stage 05**
> Mục tiêu: một trang trong `AnhEmMotor-Management` để nhân viên thấy hàng đợi, biết phiên nào AI đang
> trả lời / phiên nào người đang trả lời, nhận và trả phiên.

---

## 6.1. Vì sao không tái dùng `ChatDrawer.vue`

`ChatDrawer.vue`/`ChatFloatingButton.vue` là công cụ **AI trả lời cho chính nhân viên đang dùng nó**
(Manager Chat) — mở ở mọi trang, dạng drawer nổi, không có khái niệm "danh sách phiên của người khác".
Đây là trang quản trị xem/tham gia hội thoại của **khách hàng**, nhiều phiên cùng lúc, cần bảng danh
sách + bộ lọc + trạng thái hàng đợi — khác hẳn mục đích, nên là **route/trang riêng** trong module đã
có sẵn khái niệm khách hàng: `src/modules/Marketing/` (đã có `view/customer/pipeline`,
`view/customer/potential` — đặt cạnh đó cho hợp nhóm chức năng, ví dụ
`src/modules/Marketing/view/customer/store-chat/`).

---

## 6.2. Danh sách phiên

Bảng/danh sách với cột: tên khách (hoặc "Khách vãng lai" nếu chưa có `ContactName`), SĐT nếu có, tin
nhắn gần nhất, thời gian, badge trạng thái:

- 🟢 **AI đang trả lời** (`Mode = Ai`)
- 🟡 **Đang chờ nhân viên** (`Mode = Waiting`) — hiển thị nổi bật nhất, sắp theo thời gian chờ lâu nhất
  lên đầu
- 🔵 **Đang chat với {tên nhân viên}** (`Mode = Human`)

Bộ lọc theo đúng 3 trạng thái trên (mặc định: chỉ hiện Waiting + Human, ẩn AI để đỡ rợp — nhân viên
không cần theo dõi phiên AI đang xử lý tốt).

Nút hành động theo trạng thái: `Waiting` → "Nhận"; `Human` do chính mình phụ trách → "Trả lại AI";
`Human` do người khác phụ trách → chỉ xem, không có nút (trừ khi có quyền admin cao hơn muốn giành lại —
không làm ở Stage này, giữ đơn giản: ai nhận người đó xử lý tới khi tự trả).

---

## 6.3. Xem/tham gia hội thoại

Mở 1 phiên → hiện toàn bộ tin nhắn, render lại card sản phẩm/biến thể từ `CardsJson` (Stage 02 mục 2.3)
bằng đúng 2 component `StoreChatProductCard`/`StoreChatVariantCard` đã viết cho Store — nếu 2 project
FE không share component trực tiếp, viết bản tương đương đơn giản ở Management (chỉ hiển thị, không cần
bấm điều hướng như bên Store, tối đa là link mở tab mới tới trang sản phẩm).

Khi `Mode = Human` và mình là `AssignedStaffId`: có ô nhập liệu gửi tin nhắn trực tiếp cho khách qua
`StoreChatHub` (nhân viên join group `sessionId` sau khi claim thành công).

---

## 6.4. Realtime

Management kết nối `StoreChatHub`, join group `store-chat-staff` (đã định nghĩa ở Stage 01 mục 1.7) để
nhận `SessionUpdated` — khi 1 phiên chuyển `Waiting` (khách vừa bấm gặp nhân viên) hoặc khi 1 đồng
nghiệp vừa claim (phiên biến mất khỏi hàng đợi ngay, tránh 2 người cùng thấy 1 phiên đang chờ và giẫm
chân nhau dù Stage 05 đã có optimistic concurrency chặn ở tầng dữ liệu).

Cân nhắc thêm âm thanh/badge thông báo nhỏ khi có phiên `Waiting` mới — không bắt buộc ở bản đầu, ghi
chú lại nếu bỏ qua.

---

## Definition of Done — Stage 06

- [ ] Trang liệt kê đúng và phân biệt rõ 3 trạng thái `Ai/Waiting/Human`.
- [ ] Bấm "Nhận" trên 1 phiên `Waiting` → phiên đó biến mất khỏi hàng đợi ở **màn hình của nhân viên
      khác** gần như ngay lập tức (kiểm chứng bằng 2 tab/2 tài khoản).
- [ ] Xem lại được toàn bộ tin nhắn + card đã gửi của 1 phiên, đúng như khách đã thấy.
- [ ] Gửi tin nhắn từ trang quản trị → khách nhận được ngay trên Store (kiểm chứng 2 chiều bằng
      `Claude_Browser` mở song song Store + Management).
- [ ] Tài khoản không có permission `Marketing.StoreChatManagement.View` không truy cập được trang này.
