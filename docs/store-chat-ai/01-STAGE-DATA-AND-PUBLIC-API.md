# Stage 01 — Nền tảng dữ liệu & API công khai

> Ưu tiên: 🔴 Cao · Ước lượng: 2–3 ngày · Phụ thuộc: không
> Mục tiêu: có entity + API + Hub cho chat khách hàng chạy được (chưa cần AI thông minh), và sửa dứt
> điểm lỗi sticky của nút Message trên Store.

---

## 1.1. Vì sao không tái dùng `ChatSession`/`ChatRun` của Manager Chat

`Domain/Entities/ChatSession.cs` hiện tại bắt buộc gắn với `UserId` của nhân viên đã đăng nhập
(`ApplicationUser`), và `ChatRun`/`ChatPlan` được thiết kế xoay quanh vòng đời tool nội bộ (plan mode,
run token, `ToolRegistryFingerprint`). Khách hàng công khai trên Store phần lớn **chưa đăng nhập**, và
không cần plan mode/run engine phức tạp đó. Ép dùng chung sẽ phải thêm hàng loạt cột nullable + nhánh
rẽ if/else khắp handler hiện có — rủi ro làm hỏng luồng Manager Chat đang chạy đúng. Tách entity mới,
sạch hơn và an toàn hơn cho code cũ.

---

## 1.2. Entity mới

`Domain/Entities/StoreChatSession.cs`:

```csharp
public class StoreChatSession : BaseEntity
{
    public string VisitorKey { get; set; } = string.Empty; // GUID phía client, xem 1.4
    public Guid? CustomerUserId { get; set; }               // liên kết khi khách đã đăng nhập, nullable
    public string Mode { get; set; } = StoreChatMode.Ai;     // Ai | Waiting | Human — xem Stage 05
    public Guid? AssignedStaffId { get; set; }
    public DateTime LastMessageAt { get; set; }
    public ICollection<StoreChatMessage> Messages { get; set; } = [];
}
```

`Domain/Entities/StoreChatMessage.cs`:

```csharp
public class StoreChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public StoreChatSession Session { get; set; } = null!;
    public string Sender { get; set; } = string.Empty; // Visitor | Ai | Staff | System — StoreChatSender
    public string Content { get; set; } = string.Empty;
    public string? CardsJson { get; set; } // payload card sản phẩm/biến thể, xem Stage 02 mục 2.3
}
```

`Domain/Constants/StoreChatMode.cs` và `Domain/Constants/StoreChatSender.cs` — hằng số string theo
đúng khuôn `Domain/Constants/ChatPlanStatus.cs`/`PlanStepStatus.cs` hiện có (không dùng C# `enum` để
nhất quán với cách lưu trạng thái dạng string đã chọn cho toàn bộ nhóm Chat trong repo).

**Không thêm cột nào khác "phòng khi cần sau"** — ví dụ không thêm `Rating`, `Tags`, `Summary` ở Stage
này. Thêm khi Stage sau thực sự cần.

---

## 1.3. Migration

Tạo cặp migration cho **cả** MySQL và PostgreSQL, theo đúng mẫu `AddChatPlan`
(`Infrastructure/SqlServerMigrations/20260731123257_AddChatPlan.cs` — tên thư mục ghi "SqlServer" theo
lịch sử nhưng dự án đang chạy MySQL + PostgreSQL, kiểm tra lại provider thật trong
`Infrastructure/DBContexts` trước khi generate để dùng đúng lệnh `dotnet ef migrations add` cho từng
context). Index trên `VisitorKey` (tra cứu phiên khi khách quay lại) và `Mode` (Stage 05/06 sẽ lọc theo
cột này thường xuyên).

---

## 1.4. Định danh khách vãng lai (`VisitorKey`)

- Store FE sinh 1 GUID lần đầu (nếu `localStorage` chưa có), lưu key `store_chat_visitor_key`, gửi kèm
  mọi request REST (header hoặc query) và khi connect `StoreChatHub`.
- Khi khách đăng nhập, gắn `CustomerUserId` vào session hiện có của `VisitorKey` đó (không tạo phiên
  mới) — 1 lần gọi `PATCH`/command `LinkStoreChatSessionToCustomer` khi phát hiện đăng nhập.
- Không dùng cookie riêng, không dùng IP làm định danh chính (IP đổi liên tục, nhiều khách chung IP ở
  wifi công cộng/NAT).

---

## 1.5. Repository

`Application/Interfaces/Repositories/StoreChat/` — 4 interface tách theo đúng pattern
`Interfaces/Repositories/Chat/` hiện có:

```
IStoreChatReadRepository    // GetSessionByVisitorKeyAsync, GetSessionByIdAsync, GetHistoryAsync
IStoreChatInsertRepository  // AddSession, AddMessage
IStoreChatUpdateRepository  // (dùng ở Stage 05 cho đổi Mode/AssignedStaffId)
IStoreChatDeleteRepository  // để trống ở Stage này nếu chưa có nhu cầu xoá — không viết method rỗng vô nghĩa, bỏ hẳn interface nếu Stage 01–06 không cần, thêm lại khi có yêu cầu xoá thật
```

Implementation ở `Infrastructure/Repositories/StoreChat/`.

---

## 1.6. API công khai

`WebAPI/Controllers/V1/StoreChatController.cs` — **không** kế thừa `ApiController` nội bộ nếu class đó
mặc định yêu cầu `[Authorize]`; kiểm tra `ApiController` hiện có trước khi quyết định kế thừa hay viết
controller trần `ControllerBase` + `[AllowAnonymous]` rõ ràng trên từng action (an toàn hơn: tường minh
"action này công khai" ngay tại chỗ, không phụ thuộc việc base class có đổi default sau này).

```
POST /api/v1/store-chat/sessions           { visitorKey }               → tạo/khôi phục phiên
GET  /api/v1/store-chat/sessions/{id}/history                            → lịch sử tin nhắn
POST /api/v1/store-chat/sessions/{id}/link-customer  (khi đã đăng nhập)  → gắn CustomerUserId
```

Gửi tin nhắn **không** đi qua REST (giống quyết định "Bỏ đường REST SendMessage" đã chốt ở
`chatbot-ai/00-OVERVIEW.md` mục 5.1) — gửi qua Hub luôn, REST chỉ phục vụ tạo phiên/đọc lịch sử.

---

## 1.7. `StoreChatHub`

`WebAPI/Hubs/StoreChatHub.cs` — tái dùng pattern group-theo-session của `ManagerChatHub.cs`, khác biệt
chính: **cho phép anonymous** (không `[Authorize]`), nhóm theo `sessionId` thay vì `userId`, và có thêm
group riêng cho nhân viên đang theo dõi hàng đợi (dùng ở Stage 05/06).

```
Client → Hub: SendMessage(sessionId, content)
Hub → Client (group sessionId): ReceiveMessage(StoreChatMessageDto)
Hub → group "store-chat-staff": SessionUpdated(sessionId, mode)   // dùng ở Stage 06
```

Ở Stage này, `SendMessage` chỉ lưu tin nhắn khách + echo lại (chưa gọi AI) — nối AI thật ở Stage 02.

---

## 1.8. Rate limit

Đây là **endpoint công khai đầu tiên** của toàn hệ thống chat (Manager Chat luôn có JWT + permission
phía trước). Áp dụng rate limiting theo `VisitorKey` và theo IP ngay từ Stage này, không để dồn qua
Stage 07:

- Giới hạn số tin nhắn/phút theo `VisitorKey` (mặc định đề xuất: 20/phút — xem 00-OVERVIEW mục 6.3).
- Giới hạn số phiên mới/giờ theo IP (mặc định đề xuất: 5/giờ).
- Dùng middleware rate-limit đã có sẵn trong ASP.NET Core (`Microsoft.AspNetCore.RateLimiting`) nếu
  project chưa dùng — kiểm tra `Program.cs` xem đã có cấu hình rate limit nào cho endpoint khác chưa
  trước khi thêm cách mới.

---

## 1.9. Sửa lỗi sticky nút Message trên Store

`FloatingContact.vue` đã dùng `class="fixed bottom-20 right-4 sm:bottom-10 sm:right-6 z-1000"` — về lý
thuyết `position: fixed` là đúng cách làm sticky. Nếu người dùng báo lỗi "nút không sticky", nguyên nhân
gần như chắc chắn nằm ở **container cha**, không phải chính component này:

1. Mở Store thật bằng `Claude_Browser`/`preview_start`, cuộn trang có tái hiện được lỗi không, trên
   nhiều kích thước màn hình (`resize_window` preset mobile/tablet/desktop).
2. Nếu tái hiện được: kiểm tra `app/layouts/default.vue` và các trang bọc `<FloatingContact />` xem có
   phần tử cha nào set `overflow: hidden/auto`, `transform`, hoặc `filter` — các thuộc tính CSS này phá
   `position: fixed` khiến nó fix theo container thay vì viewport. Đây là nguyên nhân phổ biến nhất,
   sửa tại gốc (bỏ/thay thuộc tính ở container cha) thay vì đổi `FloatingContact.vue` sang
   `position: sticky` (sai công cụ — sticky vẫn cuộn theo trang, không phải thứ user muốn ở nút chat
   nổi góc màn hình).
3. Nếu không tái hiện được lỗi nào: ghi rõ trong PR là đã kiểm tra, không có gì để sửa — không đoán mò
   sửa một thứ không lỗi.

---

## Definition of Done — Stage 01

- [ ] `StoreChatSession`/`StoreChatMessage` tồn tại, có migration MySQL + PostgreSQL chạy được.
- [ ] Khách chưa đăng nhập vẫn tạo được phiên, gửi/nhận tin nhắn qua `StoreChatHub` (chưa cần AI trả
      lời thông minh, echo lại là đủ ở Stage này).
- [ ] Đóng tab, mở lại cùng trình duyệt (cùng `VisitorKey`) → thấy lại đúng lịch sử phiên cũ.
- [ ] Đăng nhập giữa chừng → phiên cũ được gắn `CustomerUserId`, không mất lịch sử.
- [ ] Rate limit hoạt động — vượt ngưỡng bị từ chối rõ ràng (429), có test.
- [ ] Lỗi sticky của nút Message: đã tái hiện + xác định nguyên nhân + sửa tại gốc (hoặc xác nhận không
      có lỗi, kèm bằng chứng đã kiểm tra).
- [ ] Không endpoint nào ở Stage này chạm được entity/table của Manager Chat (`ChatSession`, `ChatRun`,
      `ChatPlan`).
