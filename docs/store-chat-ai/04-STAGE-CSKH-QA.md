# Stage 04 — Hỏi đáp CSKH

> Ưu tiên: 🟡 Trung bình · Ước lượng: 1–2 ngày · Phụ thuộc: **Stage 02**
> Mục tiêu: AI trả lời đúng các câu hỏi CSKH cơ bản (bảo hành, đổi trả, trả góp, giờ làm việc...) mà
> không bịa, và biết khi nào nên đề nghị chuyển nhân viên thay vì đoán.

---

## 4.1. Nguồn nội dung: tái dùng FAQ tĩnh đang có, không xây CMS mới

Store đã có nội dung CSKH dạng tĩnh: `AnhEmMotor-Store/app/components/support/Categories.vue`,
`SupportFAQ` (render trong `pages/support.vue`). Đây là nguồn thật duy nhất hiện có cho câu hỏi CSKH —
không có entity `FAQ`/`Article`/`Policy` nào ở backend.

**Quyết định**: trích nội dung các câu hỏi/trả lời tĩnh này thành 1 file cấu hình
(`AISidecar/app/knowledge/store_faq.md` hoặc `.json`), nạp làm ngữ cảnh cố định vào system prompt của
persona `store` (không phải RAG/vector search — nội dung ít, cố định, không cần hạ tầng Qdrant của
Stage 12 `chatbot-ai` cho việc này). **Không xây trang CMS cho non-dev sửa FAQ ở Stage này** — nếu sau
này nội dung cần cập nhật thường xuyên hơn tốc độ deploy code, đó là lúc cân nhắc CMS, không phải bây
giờ (YAGNI).

Khi nội dung FAQ trên Store thay đổi, dev cập nhật đồng thời cả trang Vue lẫn file knowledge này — ghi
chú rõ trong comment đầu file knowledge để nhắc việc này ("giữ đồng bộ nội dung với `SupportFAQ.vue`,
sửa 1 bên thì sửa cả bên kia").

---

## 4.2. Phạm vi & ranh giới

System prompt của persona `store` phải nêu rõ:

- Được trả lời: chính sách bảo hành, đổi trả, quy trình mua/trả góp cơ bản, giờ hoạt động, thông tin
  liên hệ — đúng những gì có trong `store_faq.md`.
- **Không** được bịa số liệu, chính sách, hoặc cam kết không có trong nguồn — nếu câu hỏi nằm ngoài
  `store_faq.md` và không phải câu hỏi tra sản phẩm (Stage 02), trả lời an toàn kiểu "mình chưa có
  thông tin chắc chắn về việc này, để mình chuyển bạn qua nhân viên nhé" + gợi ý nút "Gặp nhân viên"
  (nối với Stage 05).
- Không tư vấn tài chính/pháp lý cụ thể (vd. không tự tính lãi suất trả góp chính xác cho từng khách
  nếu backend chưa có tool tính toán riêng) — chỉ nêu thông tin chung đã có trong FAQ.

---

## Definition of Done — Stage 04

- [ ] Hỏi ≥ 5 câu CSKH có trong FAQ hiện tại (bảo hành, đổi trả, giờ mở cửa, liên hệ, trả góp cơ bản) →
      AI trả lời đúng nội dung, không bịa thêm.
- [ ] Hỏi 1 câu CSKH rõ ràng ngoài phạm vi FAQ → AI từ chối đoán, đề nghị chuyển nhân viên.
- [ ] Câu hỏi lẫn giữa sản phẩm và CSKH ("xe SH có bảo hành mấy năm") → AI dùng đúng tool sản phẩm
      (Stage 02) kết hợp nội dung FAQ, không chỉ chọn 1 trong 2.
