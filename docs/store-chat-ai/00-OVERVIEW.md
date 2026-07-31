# Store Chat AI — Kế hoạch hoàn thiện (Overview)

> Cập nhật: 2026-07-31
> Nhánh em của [`docs/chatbot-ai/`](../chatbot-ai/00-OVERVIEW.md) (Manager Chat) — dùng chung sidecar,
> khác tuyến người dùng: đây là chat AI cho **khách hàng công khai trên Store**, không phải nhân viên.

Mục lục cho toàn bộ kế hoạch Store Chat AI. Mỗi Stage là một file riêng, mỗi Stage nên là một PR.

> ⚠️ **Số hiệu file là thứ tự thực hiện đề xuất** (khác với `chatbot-ai/`, ở đây không có phụ thuộc
> chéo phức tạp nên đánh số luôn theo thứ tự làm).

---

## 1. Vì sao có tài liệu này, và vì sao tách khỏi `chatbot-ai/`

Yêu cầu nghiệp vụ: khách vào Store (chưa chắc đăng nhập) chat được với AI để tìm xe theo mô tả tự
nhiên, bấm card ra đúng biến thể màu, hỏi CSKH cơ bản, và khi AI không xử lý được thì có nhân viên tiếp
nhận qua một trang quản trị riêng.

Đây **không phải** mở rộng của Manager Chat dù dùng chung hạ tầng AI, vì đối tượng người dùng, dữ liệu
được phép chạm vào, và mô hình phiên khác hẳn nhau:

| | Manager Chat (đã có) | Store Chat (tài liệu này) |
|---|---|---|
| Người dùng | Nhân viên đã đăng nhập, có permission | Khách vãng lai công khai (có thể chưa đăng nhập) |
| Dữ liệu AI được chạm | 71+ tool nội bộ: đơn hàng, khách hàng, **lương/hoa hồng**, tồn kho chi tiết | Chỉ đọc: tìm/xem sản phẩm, biến thể, tồn kho ở mức còn/hết, FAQ CSKH |
| Định danh phiên | `ChatSession.UserId` (bắt buộc có tài khoản) | `VisitorKey` ẩn danh (localStorage) + tuỳ chọn liên kết tài khoản khách khi đăng nhập |
| Ai xử lý khi AI không đủ | Không có khái niệm chuyển người | Chuyển cho nhân viên CSKH qua hàng đợi claim |
| Nơi quản trị | `ChatDrawer.vue` (chính là công cụ chat, không phải trang xem session người khác) | Trang quản trị riêng: danh sách phiên, ai đang trả lời (AI/người), nhận/trả phiên |

Vì entity, quyền, và luồng dữ liệu tách bạch hoàn toàn, **không** tái dùng `ChatSession`/`ChatRun`/
`ChatPlan`/`ManagerChatHub`/`InternalChatToolsController` — chỉ tái dùng những gì thực sự là hạ tầng
dùng chung (tiến trình sidecar Python, khung tool-calling LangGraph, các Query Handler đọc sản phẩm).

---

## 2. Trạng thái hiện tại (đã xác nhận bằng code — không phải suy đoán)

### Đã có (tái dùng được)
- **Sidecar AI dùng chung**: `AISidecar/` (FastAPI + LangChain/LangGraph, Gemini), do
  `Infrastructure/Services/Ai/AiSidecarManager.cs` spawn 1 tiến trình duy nhất, chỉ bind
  `127.0.0.1`. Khung tool-calling (`create_react_agent`) đã hoàn thiện qua Stage 03/07 của
  `chatbot-ai/done/`.
- **DTO 2 cấp sản phẩm/biến thể đúng khớp yêu cầu**: `ChatProductDetailDto` (giá từ-đến, brand,
  category) + `ChatProductVariantDetailDto` (VariantId, tên màu, SKU, giá) —
  `Application/Features/ChatTools/Queries/GetProductDetailForChat/ChatProductDetailDto.cs`.
- **Tìm sản phẩm đã có fallback gõ sai/thiếu dấu tiếng Việt** — xem test
  `Search_KeywordCoDauNhungTenSanPhamThieuDau_VanTimDuocQuaFallback` trong
  `UnitTests/ChatTools.cs`, handler `SearchProductsForChatQueryHandler`.
- **Pattern Hub SignalR + luồng streaming** đã chứng minh chạy được ở `WebAPI/Hubs/ManagerChatHub.cs`.
- **Nội dung CSKH có sẵn dạng tĩnh**: `AnhEmMotor-Store/app/components/support/*`,
  `pages/support.vue` (SupportFAQ, SupportCategories) — dùng làm ngữ cảnh ban đầu cho AI, chưa cần
  CMS mới.
- **Mô hình permission "cấp riêng, không gán mặc định role nào"** đã có tiền lệ ở
  [`chatbot-ai/21-STAGE-ADMIN-CHAT-HISTORY.md`](../chatbot-ai/21-STAGE-ADMIN-CHAT-HISTORY.md) —
  dùng lại đúng nguyên tắc đó cho quyền nhận phiên chat Store.

### Chưa có (phải xây mới)
| # | Thiếu gì | Stage |
|---|---|---|
| 1 | Không entity/bảng nào lưu phiên chat khách hàng | 01 |
| 2 | Không API/Hub công khai nào cho chat (mọi thứ hiện có đều yêu cầu JWT nhân viên) | 01 |
| 3 | `FloatingContact.vue` ở Store chỉ là giao diện tĩnh — tin nhắn hard-code, input không hoạt động | 01, 03 |
| 4 | Nút Message có thể đang lỗi sticky trên một số trang/kích thước màn hình — chưa xác định nguyên nhân | 01 |
| 5 | Không có bộ tool AI nào an toàn để công khai (tool hiện có đều gắn `[HasPermission]` nội bộ) | 02 |
| 6 | Store chưa hỗ trợ mở thẳng đúng biến thể màu qua URL (chỉ đọc `route.params.slug`) | 03 |
| 7 | Không có cơ chế AI trả lời CSKH ngoài phạm vi sản phẩm | 04 |
| 8 | Không có khái niệm chuyển phiên AI → người, không hàng đợi, không permission nhận phiên | 05 |
| 9 | Không trang quản trị nào cho phiên chat Store (khác hẳn `ChatDrawer.vue`) | 06 |
| 10 | Chưa có rate-limit/guardrail cho một endpoint **công khai không xác thực** — rủi ro bảo mật mới hoàn toàn so với Manager Chat | 07 |

---

## 3. Danh sách Stage

| ID | Tên | File | Ước lượng |
|---|---|---|---|
| 01 | Nền tảng dữ liệu & API công khai | [01-STAGE-DATA-AND-PUBLIC-API.md](01-STAGE-DATA-AND-PUBLIC-API.md) | 2–3 ngày |
| 02 | Bộ tool AI cho khách hàng (dùng chung sidecar) | [02-STAGE-AI-TOOL-SCOPE.md](02-STAGE-AI-TOOL-SCOPE.md) | 2–3 ngày |
| 03 | Giao diện chat & liên kết biến thể trên Store | [03-STAGE-STORE-WIDGET-AND-VARIANT-LINK.md](03-STAGE-STORE-WIDGET-AND-VARIANT-LINK.md) | 2 ngày |
| 04 | Hỏi đáp CSKH | [04-STAGE-CSKH-QA.md](04-STAGE-CSKH-QA.md) | 1–2 ngày |
| 05 | Chuyển tiếp nhân viên (handoff) | [05-STAGE-HANDOFF-TO-STAFF.md](05-STAGE-HANDOFF-TO-STAFF.md) | 2–3 ngày |
| 06 | Trang quản trị phiên chat Store | [06-STAGE-ADMIN-CONSOLE.md](06-STAGE-ADMIN-CONSOLE.md) | 2–3 ngày |
| 07 | Bảo mật, giới hạn & vận hành | [07-STAGE-SECURITY-AND-OPERATIONS.md](07-STAGE-SECURITY-AND-OPERATIONS.md) | 1–2 ngày |

**Tổng ước lượng: ~12–18 ngày công.**

---

## 4. Thứ tự thực hiện & mốc bàn giao

```
01 Data & API công khai  →  02 Tool AI  →  03 Widget & variant link  →  04 CSKH
   →  05 Handoff  →  06 Trang quản trị  →  07 Bảo mật & vận hành
```

| Mốc | Sau Stage | Người dùng nhận được |
|---|---|---|
| **M1 — Chat công khai chạy được** | 01 | Khách gõ tin nhắn, tin được lưu, hạ tầng realtime hoạt động (AI chưa thông minh) |
| **M2 — Gợi ý sản phẩm thật** | 02, 03 | Gõ sai dấu vẫn ra đúng xe; bấm card ra đúng trang biến thể màu |
| **M3 — Trợ lý CSKH** | 04 | Hỏi bảo hành/đổi trả/trả góp cơ bản được trả lời đúng |
| **M4 — Có người thật khi cần** | 05, 06 | AI chuyển đúng lúc; nhân viên nhận/trả phiên qua trang quản trị |
| **M5 — Sẵn sàng production** | 07 | Chống spam/injection, không rò tool nội bộ, có audit log |

---

## 5. Quyết định đã chốt

| # | Quyết định | Lý do |
|---|---|---|
| 1 | Chia nhiều file MD theo Stage + 1 file `00-OVERVIEW.md` | Giữ đúng quy ước đang dùng ở `chatbot-ai/`, dễ review từng phần |
| 2 | Dùng chung sidecar Python của Manager Chat, thêm route/tool-catalog/guardrail riêng cho khách hàng | Tái dùng khung LangGraph + streaming đã ổn định, tránh dựng lại hạ tầng AI thứ hai |
| 3 | Nhân viên nhận phiên theo **hàng đợi tự nhận (claim)**, permission mới không gán mặc định role nào | Đơn giản hơn quản lý ca trực, đúng tiền lệ bảo mật đã dùng ở Stage 21 `chatbot-ai/` |

## 6. Quyết định còn mở (chốt khi bắt đầu code từng Stage)

| # | Quyết định | Mặc định đề xuất | Chốt ở Stage |
|---|---|---|---|
| 1 | Tên tham số deep-link biến thể trên URL | `?variant=<variantId>` | 03 |
| 2 | Định danh khách vãng lai | GUID sinh phía client, lưu `localStorage`, gửi kèm mọi request/hub connection dưới tên `VisitorKey` | 01 |
| 3 | Ngưỡng rate-limit cụ thể (số tin/phút, số phiên/IP) | Khởi điểm: 20 tin/phút/`VisitorKey`, 5 phiên mới/giờ/IP — chỉnh theo số liệu thực tế sau khi bật | 07 |
| 4 | Có bắt buộc nhập tên/SĐT khi chuyển nhân viên không | Không bắt buộc để bắt đầu chat, nhưng **bắt buộc** trước khi claim thành công (nhân viên cần biết gọi lại cho ai) | 05 |
| 5 | Namespace permission mới | `Permissions.Marketing.StoreChatManagement.{View,Claim}` (đặt trong module `Marketing` vì đã có `Marketing/CustomerCareManagement.cs`, `Marketing/customer/*` — không tạo module top-level mới) | 05 |

---

## 7. Quy ước chung (kế thừa từ `chatbot-ai/`)

- **Ngôn ngữ**: comment tiếng Việt, code tiếng Anh.
- **Backend**: CQRS — mỗi thao tác 1 Command/Query + Handler riêng thư mục, dưới namespace mới
  `Application/Features/StoreChat/` (tách hẳn khỏi `Features/ManagerChat/`).
- **Repository**: tách 4 interface `IStoreChatRead/Insert/Update/DeleteRepository`, theo mẫu
  `Interfaces/Repositories/Chat/`.
- **Sidecar**: business logic AI ở Python; .NET orchestrate + persist; LLM không bao giờ chạm DB
  trực tiếp.
- **Permission**: kiểm tra ở backend .NET, không dựa vào prompt.
- **Migration**: luôn tạo cho cả MySQL và PostgreSQL.
- **Mỗi Stage 1 PR**, phải pass Definition of Done ở cuối file Stage.

---

## 8. Nghiệm thu tổng thể

- [ ] Gõ "sh 2024 màu gì đẹp" (sai/thiếu dấu tuỳ ý) → AI vẫn gợi ý đúng xe từ dữ liệu tồn kho thật.
- [ ] Bấm vào card biến thể màu → mở đúng trang sản phẩm, đúng màu đã chọn sẵn.
- [ ] Hỏi "chính sách bảo hành thế nào" → AI trả lời đúng nội dung CSKH hiện có, không bịa.
- [ ] Hỏi thứ AI không xử lý được (khiếu nại, đàm phán giá) → AI đề nghị chuyển nhân viên, không cố trả lời liều.
- [ ] Bấm "Gặp nhân viên" → phiên vào hàng đợi, nhân viên có quyền thấy và bấm "Nhận" là nhận được.
- [ ] Nhân viên bấm "Trả lại AI" → AI tiếp tục trả lời bình thường.
- [ ] Trang quản trị phân biệt rõ phiên nào AI đang trả lời / phiên nào người đang trả lời / phiên đang chờ.
- [ ] Request từ persona Store **không** gọi được bất kỳ tool nội bộ nào của Manager Chat (đơn hàng, lương, khách hàng) — có test khoá lại.
- [ ] Nút Message không còn lỗi sticky trên mọi kích thước màn hình đã kiểm tra.
- [ ] Có rate-limit chống spam, không cần đăng nhập vẫn chat được.
