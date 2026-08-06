# Stage 05 — Chuyển tiếp nhân viên (handoff)

> Ưu tiên: 🔴 Cao · Ước lượng: 2–3 ngày · Phụ thuộc: **Stage 01, 02, 04**
> Mục tiêu: phiên chat chuyển đúng lúc từ AI sang người, qua mô hình hàng đợi tự nhận (claim), với
> permission mới không gán mặc định cho role nào.

---

## 5.1. `Mode` của `StoreChatSession` — 3 trạng thái

`Domain/Constants/StoreChatMode.cs` (đã khai báo entity ở Stage 01, Stage này định nghĩa giá trị và
luồng chuyển):

```
Ai       — mặc định, AI đang trả lời
Waiting  — đã yêu cầu chuyển người, đang chờ nhân viên nhận
Human    — một nhân viên cụ thể (AssignedStaffId) đang trả lời trực tiếp
```

Chuyển `Ai → Waiting` khi:

1. Khách bấm nút "Gặp nhân viên" (luôn hiển thị sẵn trong khung chat, không phụ thuộc AI có gợi ý hay
   không) — **hành động rõ ràng của khách, ưu tiên cao nhất, không cần AI đồng ý**.
2. AI tự gọi tool `escalate_to_staff` (thêm vào catalog persona `store`, đây là **tool ghi duy nhất**
   được phép ở persona này — chỉ đổi `Mode`, không chạm dữ liệu nghiệp vụ nào khác) khi nhận diện câu
   hỏi ngoài khả năng: khiếu nại, đàm phán giá, yêu cầu CSKH phức tạp không có trong FAQ (nối với Stage
   04 mục 4.2).

Chuyển `Waiting → Human`: nhân viên bấm "Nhận" (mục 5.3).

Chuyển `Human → Ai`: nhân viên bấm "Trả lại AI" (mục 5.3) — về lại `Ai`, không quay lại `Waiting`.

Khi `Mode != Ai`: AI **ngừng trả lời** tin nhắn mới của khách trong phiên đó (sidecar không nhận thêm
tin từ phiên này cho tới khi có nhân viên trả lại) — kiểm tra `Mode` ở `StoreChatHub.SendMessage` trước
khi quyết định forward tin nhắn cho sidecar hay chỉ lưu + phát cho nhóm nhân viên.

---

## 5.2. Thông tin liên hệ trước khi claim

Quyết định mặc định (00-OVERVIEW mục 6.4): không bắt buộc nhập tên/SĐT để bắt đầu chat, nhưng **bắt
buộc trước khi một nhân viên claim thành công** — nếu phiên chưa có tên + SĐT (hoặc `CustomerUserId`
đã đăng nhập, tự có sẵn thông tin), khi khách bấm "Gặp nhân viên" thì hiện form ngắn xin tên/SĐT trước,
rồi mới chuyển `Waiting`. Lưu vào `StoreChatSession` (thêm 2 cột `ContactName`, `ContactPhone`, nullable
— chỉ thêm ở Stage này khi thực sự cần, không thêm sẵn ở Stage 01).

---

## 5.3. Permission mới — nguyên tắc giống Stage 21 `chatbot-ai`

Đây là tính năng cho phép nhân viên **đọc và tham gia hội thoại của khách hàng công khai** — áp dụng
đúng nguyên tắc đã dùng cho `Admin.ChatHistoryManagement.View`:

1. **Permission mới, không tái dùng** `Marketing.CustomerCareManagement.*` đã có (permission đó phục vụ
   mục đích khác, không phải để nhận chat trực tiếp). Tạo
   `Domain/Constants/Permission/Permissions/Marketing/StoreChatManagement.cs`:

   ```csharp
   namespace Domain.Constants.Permission;

   public static partial class Permissions
   {
       public static partial class Marketing
       {
           public static class StoreChatManagement
           {
               public const string View = "Permissions.Marketing.StoreChatManagement.View";   // xem hàng đợi + nhận
               public const string Claim = "Permissions.Marketing.StoreChatManagement.Claim";  // tự nhận phiên
           }
       }
   }
   ```

2. **Không gán mặc định cho role nào** — cấp tay qua màn hình quản lý role hiện có, giống Stage 21.
3. Endpoint claim/release kiểm tra permission `Claim`, endpoint xem danh sách/transcript (dùng ở Stage
   06) kiểm tra permission `View`.

---

## 5.4. API

`Application/Features/StoreChat/Commands/` (namespace mới, tách khỏi `ManagerChat/` và khỏi
`StoreChat/Queries` đọc thường):

```
RequestHandoffCommand(SessionId, ContactName?, ContactPhone?)     → Ai/... → Waiting
ClaimStoreChatSessionCommand(SessionId)                            → Waiting → Human, AssignedStaffId = current user
ReleaseStoreChatSessionCommand(SessionId)                          → Human → Ai, AssignedStaffId = null
```

`ClaimStoreChatSessionCommandHandler` phải xử lý race condition (2 nhân viên bấm "Nhận" gần như đồng
thời) — dùng optimistic concurrency (cột `Version` giống `ChatPlan.Version`, hoặc kiểm tra `Mode ==
Waiting` trong cùng transaction/`WHERE` clause khi update) để chỉ 1 người nhận thành công, người còn lại
nhận lỗi Conflict rõ ràng thay vì cả hai đều tưởng mình đã nhận.

Endpoint tương ứng trong `StoreChatController` (hoặc controller mới
`StoreChatHandoffController` nếu muốn tách bạch quyền hạn ở tầng route cho dễ audit, theo đúng lý do
Stage 21 đã tách `AdminChatHistoryController` riêng khỏi `ManagerChatController`).

---

## Definition of Done — Stage 05

- [ ] Bấm "Gặp nhân viên" → phiên chuyển `Waiting`, AI ngừng trả lời phiên đó.
- [ ] AI tự gọi `escalate_to_staff` đúng lúc với câu hỏi ngoài khả năng (test theo kịch bản Stage 04).
- [ ] Permission `Marketing.StoreChatManagement.{View,Claim}` tồn tại, **không** gán mặc định cho role
      nào — có test xác nhận tài khoản không có quyền bị từ chối (403).
- [ ] 2 nhân viên bấm "Nhận" gần như đồng thời trên cùng phiên → chỉ 1 người nhận thành công, có test.
- [ ] Nhân viên bấm "Trả lại AI" → `Mode` về `Ai`, AI tiếp tục trả lời bình thường ngay tin nhắn kế
      tiếp.
- [ ] Chưa có tên/SĐT → bị chặn nhận (claim) cho tới khi khách cung cấp.
