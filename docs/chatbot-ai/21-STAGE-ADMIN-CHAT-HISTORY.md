# Stage 21 — Trang quản trị: Lịch sử chat & phản hồi số liệu

> Ưu tiên: 🟡 Thấp · Ước lượng: 2–3 ngày · Phụ thuộc: **Stage 8** (`ChatRunEvent`), **Stage 16**
> (`ChatFeedback`, nút "Số liệu chưa đúng")
> Mục tiêu: một màn hình admin, **quyền riêng biệt**, xem được lịch sử chat của **mọi người dùng**
> (không chỉ của chính mình) — feedback "Số liệu chưa đúng" là một bộ lọc trong màn hình này, không
> phải một trang riêng.

---

## 21.1. Vì sao gộp chung, không tách 2 trang

Bản đầu của Stage này (rev 2026-07-30) chỉ định làm 1 API + màn hình liệt kê `ChatFeedback`. Mở rộng
theo yêu cầu: nếu chỉ xem được feedback thì không đủ ngữ cảnh — người xem cần thấy **toàn bộ hội thoại**
xung quanh feedback đó, và cũng cần xem lịch sử chat của người dùng bất kỳ khi điều tra sự cố **kể cả
khi họ chưa bấm feedback nào**. Một màn hình "Lịch sử chat toàn hệ thống" với bộ lọc "chỉ hiện có
feedback" phủ được cả hai nhu cầu, không cần 2 trang trùng lặp phần lớn logic.

---

## 21.2. Ranh giới quyền — điểm quan trọng nhất của Stage này

Đây là tính năng đọc được **nội dung chat riêng tư của người dùng khác** — khác hẳn mọi permission
khác trong hệ thống vốn chỉ giới hạn theo *module nghiệp vụ*, không phải theo *người khác đã nói gì*.

**Nguyên tắc bắt buộc:**
1. Permission mới, không tái dùng bất kỳ permission nào đã có: `Permissions.Admin.ChatHistoryManagement.View`.
   Tạo file `Domain/Constants/Permission/Permissions/Admin/ChatHistoryManagement.cs` theo đúng khuôn các
   permission `Admin.*` hiện có (xem `Admin/DashboardManagement.cs`).
2. **Không gán mặc định cho role nào** (kể cả Administrator seed sẵn) — phải cấp tay qua màn hình quản
   lý role, để việc cấp quyền này luôn là một quyết định có chủ đích, có thể audit ai đã cấp cho ai.
3. **Tuyệt đối không sửa** `GetManagerChatSessionsQueryHandler`, `GetManagerChatSessionHistoryQueryHandler`,
   `GetChatRunEventsQueryHandler` hiện có — các handler này đang đúng (chỉ cho xem session của chính
   mình), đã có test khoá lại (`MCHAT_05 - Không thể xem lịch sử phiên chat của người khác` trong
   `IntegrationTests/ManagerChat.cs`). Bất kỳ thay đổi nào ở đó là nới lỏng bảo mật cho **toàn bộ**
   người dùng thường, không chỉ cho tính năng admin này.
4. Viết **Query/Handler mới hoàn toàn**, kiểm tra permission mới thay vì kiểm tra `Session.UserId ==
   currentUserId` — hai luồng (user tự xem / admin xem người khác) không dùng chung code path.
5. **Ghi audit log** mỗi lần admin mở lịch sử chat của người khác: ai xem, xem của user nào, session
   nào, lúc nào. Đây là hành động nhạy cảm quyền riêng tư, phải truy vết ngược được nếu bị lạm dụng.

---

## 21.3. Backend

`Application/Features/AdminChatHistory/` (namespace mới, tách khỏi `ManagerChat/` để không lẫn với
code path của user thường — xem nguyên tắc #4 ở trên):

### Queries

```
GetAllChatSessionsQuery(UserId?, FromDate?, ToDate?, OnlyWithFeedback bool = false, Page, PageSize)
  → PagedResult<AdminChatSessionListItemDto>
    (SessionId, UserId, UserName, Title, CreatedAt, UpdatedAt, HasFeedback, FeedbackCount)

GetChatSessionHistoryAsAdminQuery(SessionId)
  → List<ChatMessageDto>   // tái dùng DTO đã có của GetManagerChatSessionHistoryQuery, chỉ khác
                            // Handler bỏ qua check ownership, thay bằng check permission mới

GetChatRunEventsAsAdminQuery(RunId)
  → ChatRunEventsResult    // tái dùng logic của GetChatRunEventsQueryHandler (dựng lại tool đã gọi/
                            // tham số/kết quả), chỉ khác điều kiện truy cập
```

`GetAllChatSessionsQueryHandler` join `ChatSession` → `ChatRun` → `ChatFeedback` (LEFT JOIN, đếm số
feedback) để trả `HasFeedback`/`FeedbackCount` mà không cần round-trip riêng. Thêm 1 method vào
`IChatReadRepository` (hoặc interface admin riêng nếu muốn tách hẳn) — không viết SQL tay mới trùng
lặp, tái dùng cách join đã có trong `GetChatFeedbackList` nếu Stage này làm sau khi phần feedback-list
cũ đã có (xem lịch sử revision file này).

### Endpoint

`WebAPI/Controllers/V1/AdminChatHistoryController.cs` (controller mới, **không** thêm action vào
`ManagerChatController` — giữ tách bạch quyền hạn ở tầng route cho dễ audit/rà soát sau này):

```
GET /api/v1/admin/chat-history/sessions?userId=&fromDate=&toDate=&onlyWithFeedback=&page=&pageSize=
GET /api/v1/admin/chat-history/sessions/{sessionId}/history
GET /api/v1/admin/chat-history/runs/{runId}/events
```

Cả 3 action gắn `[HasPermission(Permissions.Admin.ChatHistoryManagement.View)]`.

### Audit log

Ghi vào bảng log hiện có của hệ thống (kiểm tra xem đã có audit log chung nào — ví dụ theo mẫu
`BannerAuditLog`/`CommissionPolicyAuditLog` đã thấy trong `Domain/Entities/`) hoặc structured log
riêng nếu chưa có audit log framework dùng chung:

```csharp
logger.LogWarning(
    "[AdminChatHistory] AdminUserId={AdminUserId} xem lịch sử SessionId={SessionId} của UserId={TargetUserId}",
    currentUserId, sessionId, session.UserId);
```

---

## 21.4. Frontend

Trang mới trong `AnhEmMotor-Management`, route riêng dưới nhóm Admin/Quản trị hệ thống hiện có:

- **Danh sách session** — bảng: người dùng, tiêu đề, thời gian tạo/cập nhật, badge "🚩 N feedback" nếu
  `HasFeedback`. Bộ lọc: theo người dùng (dropdown/search), theo khoảng ngày, checkbox "chỉ hiện có
  feedback".
- **Xem chi tiết** — mở trang/modal readonly hiển thị toàn bộ tin nhắn của session (dùng lại
  `marked.parse` + style bubble đã có trong `ChatDrawer.vue`, nhưng **không có** ô nhập liệu/nút gửi —
  đây là màn hình chỉ đọc). Feedback nào thuộc session này thì hiện chú thích ngay tại tin nhắn tương
  ứng (dựa `ChatRunId` khớp).
- **Xem tool đã gọi** — click vào một đoạn trả lời của AI → hiện lại đúng tool/tham số/kết quả (dùng
  `GetChatRunEventsAsAdminQuery`), giống panel "N công cụ đã dùng" trong `ChatDrawer.vue` nhưng không
  cần realtime (SignalR) vì đây là dữ liệu đã hoàn tất, load một lần qua REST là đủ.

Nếu chưa có thời gian làm UI đầy đủ ngay: bản tối thiểu chấp nhận được là chỉ có 3 API ở mục 21.3, đội
dev/support tra qua Swagger với tài khoản đã được cấp quyền — ghi rõ trong PR đây là bản rút gọn.

---

## 21.5. Rủi ro cần cân nhắc trước khi bật ở production

- Đây là tính năng đọc được dữ liệu chat của **mọi người dùng** — nếu công ty có chính sách bảo mật/
  quyền riêng tư nội bộ, cần xác nhận với người phụ trách trước khi cấp quyền này cho bất kỳ ai, kể cả
  quản trị viên hệ thống.
- Cân nhắc áp dụng lại quy tắc redaction đã có ở Stage 11 (ẩn tham số/kết quả tool thô ở production)
  cho cả màn hình này, nếu nội dung tool có thể chứa dữ liệu nhạy cảm hơn mức cần thiết để điều tra.
- Không mở rộng phạm vi permission này sang được **sửa/xoá** dữ liệu chat của người khác — chỉ đọc.

---

## Definition of Done — Stage 21

- [ ] Permission `Permissions.Admin.ChatHistoryManagement.View` tồn tại, **không** gán mặc định cho
      role nào (kể cả Administrator).
- [ ] `GetManagerChatSessionsQueryHandler`/`GetManagerChatSessionHistoryQueryHandler`/
      `GetChatRunEventsQueryHandler` hiện có **không đổi hành vi** — `MCHAT_05` và các test liên quan
      vẫn xanh sau khi thêm Stage này.
- [ ] Trang admin liệt kê được session của **mọi** người dùng, lọc theo người dùng/ngày/có feedback.
- [ ] Xem được toàn bộ tin nhắn + tool đã gọi của bất kỳ session nào (đúng quyền mới).
- [ ] Tài khoản **không có** permission mới bị từ chối (403) khi gọi 3 endpoint admin — có test riêng.
- [ ] Mỗi lần admin mở lịch sử của người khác đều có audit log (ai, xem của ai, lúc nào).
- [ ] Feedback "Số liệu chưa đúng" hiển thị/lọc được ngay trong màn hình này, không cần trang riêng.
