# Stage 4 — Hoàn thiện UX & Frontend

> Ưu tiên: 🟠 Trung bình · Ước lượng: 2–3 ngày · Phụ thuộc: Stage 1 (bắt buộc), Stage 3 (cho mục 4.5)
> Mục tiêu: chatbot dùng thật thấy mượt, không "chạy được là xong".

**File chính:** `AnhEmMotor-Management/src/components/business/chat/ChatDrawer.vue` (641 dòng)
**API:** `AnhEmMotor-Management/src/api/chat/chat.api.ts`

---

## 4.1. Sinh tiêu đề phiên chat bằng LLM

**Hiện tại:** `AISidecar/controllers/manager_chat_controller.py`

```python
@router.post("/manager-chat/generate-title")
async def generate_title(req: GenerateTitleRequest):
    title = req.message[:30].strip() + ("..." if len(req.message) > 30 else "")
    return {"title": title}
```

Chỉ cắt chuỗi, và **FE hiện chưa gọi endpoint này**.

### Sidecar — dùng LLM thật

```python
@router.post("/manager-chat/generate-title")
async def generate_title(req: GenerateTitleRequest,
                         _: str = Depends(verify_internal_header)):
    llm = get_llm(temperature=0.3)
    prompt = [
        SystemMessage(content=(
            "Đặt tiêu đề ngắn gọn cho đoạn hội thoại, tối đa 6 từ, tiếng Việt. "
            "Chỉ trả về đúng tiêu đề, không giải thích, không dấu ngoặc kép."
        )),
        HumanMessage(content=req.message),
    ]
    try:
        result = await llm.ainvoke(prompt)
        title = (result.content or "").strip().strip('"')[:60]
        if title:
            return {"title": title}
    except Exception:
        pass
    # Fallback: cắt chuỗi như cũ
    fallback = req.message[:30].strip()
    return {"title": fallback + ("..." if len(req.message) > 30 else "")}
```

### Backend — command mới

Tạo `Application/Features/ManagerChat/Commands/GenerateManagerChatSessionTitle/`:
- Gọi sidecar `/manager-chat/generate-title`.
- Cập nhật `session.Title` qua `IChatUpdateRepository`.
- Trả title mới.

Endpoint: `POST /api/v1/manager-chat/sessions/{id}/generate-title`

### Frontend — tự động gọi

Trong `ChatDrawer.vue`: sau khi tin nhắn **đầu tiên** của session được trả lời xong (stream complete)
và `session.title` đang rỗng / mặc định → gọi `generateTitle` và cập nhật tên trong sidebar.

---

## 4.2. Nút dừng (Stop generation)

**Hiện tại:** đã stream nhưng không dừng được giữa chừng.

- **FE:** `stream.subscribe(...)` trả về `ISubscription` — giữ tham chiếu, gọi `.dispose()` khi bấm Stop.
  Đổi nút "Gửi" thành "Dừng" khi đang stream.
- **Backend:** `ManagerChatHub.SendMessageStream` đã nhận `CancellationToken` — SignalR tự cancel
  khi client dispose subscription.
- **Handler:** `StreamManagerChatMessageCommandHandler` đã có `if (cancellationToken.IsCancellationRequested) break;`
  → **nhưng phần lưu `aiMessage` sau vòng lặp sẽ dùng token đã cancel** → `SaveChangesAsync(cancellationToken)`
  ném exception, mất nội dung đã stream.

  **Sửa:** lưu phần đã stream được với `CancellationToken.None`:
  ```csharp
  chatInsertRepository.AddMessage(aiMessage);
  await unitOfWork.SaveChangesAsync(CancellationToken.None);
  ```
  Cân nhắc đánh dấu tin nhắn bị ngắt (ví dụ append `" [đã dừng]"` hoặc thêm cột `IsInterrupted`).

---

## 4.3. Trạng thái lỗi rõ ràng

**Hiện tại:** lỗi từ AI provider được **yield thẳng vào nội dung tin nhắn**:

```python
yield f"\n[Lỗi kết nối tới AI Provider: {str(e)}]"
```

→ Chuỗi lỗi (có thể chứa chi tiết nội bộ, thậm chí một phần API key trong message) bị **lưu vào DB**
và hiển thị cho user như thể AI nói vậy.

**Sửa:**
1. Sidecar: log exception đầy đủ ở server, chỉ gửi ra ngoài thông điệp chung:
   ```json
   {"type":"error","message":"Không kết nối được tới dịch vụ AI. Vui lòng thử lại."}
   ```
2. Backend: **không** nối phần `error` vào `fullReply` lưu DB.
3. FE: render dạng banner đỏ trong bubble + nút "Thử lại", không phải text thường.

---

## 4.4. Render Markdown

AI trả markdown (bảng, danh sách, code) nhưng FE hiện render text thô.

- Thêm `markdown-it` + `dompurify` (kiểm tra `package.json` xem đã có chưa).
- **Bắt buộc sanitize** trước khi `v-html` — nội dung do LLM sinh là untrusted.
- Cấu hình: cho phép `p, ul, ol, li, strong, em, code, pre, table, thead, tbody, tr, th, td, a, br, h1-h4`.
  Chặn `script, iframe, style, on*` attribute.
- Với `<a>`: ép `target="_blank" rel="noopener noreferrer"`.

---

## 4.5. Indicator khi AI đang gọi tool

*(Chỉ làm sau khi Stage 3 xong)*

Khi nhận `{"type":"tool_start","name":"get_sales_summary"}` → hiện dòng nhỏ dưới bubble:
> 🔍 Đang tra cứu doanh thu...

Map tên tool → nhãn tiếng Việt trong một object constant ở FE.

---

## 4.6. Các cải thiện UX khác

| Hạng mục | Mô tả |
|---|---|
| Auto-scroll thông minh | Chỉ auto-scroll khi user đang ở đáy; nếu đã scroll lên đọc thì hiện nút "↓ Tin nhắn mới" |
| Empty state | Session mới → hiện 3–4 câu hỏi gợi ý bấm được ("Doanh thu tuần này?", "Sản phẩm sắp hết hàng") |
| Trạng thái kết nối SignalR | Badge nhỏ: Đang kết nối / Đã kết nối / Mất kết nối + auto-reconnect (`withAutomaticReconnect()`) |
| Copy tin nhắn | Nút copy trên bubble của AI |
| Xác nhận khi xoá session | Modal confirm, hiện đang cần kiểm tra xem đã có chưa |
| Phím tắt | `Enter` gửi, `Shift+Enter` xuống dòng, `Esc` đóng drawer |
| Loading skeleton | Khi load history session dài |
| Giới hạn input | Max ~4000 ký tự, hiện counter khi gần chạm |
| Responsive | Drawer full-width trên màn hình < 768px |

---

## 4.7. Dọn `chat.api.ts`

- Bỏ `sendMessage()` nếu chọn Hướng A ở Stage 1.
- Thêm `generateTitle(sessionId)`.
- Sửa `ChatRole` type cho khớp backend (đã nêu ở Stage 1.3).
- Bổ sung type cho `ChatSession`: thiếu `createdAt`, `updatedAt` (dùng để sắp xếp sidebar).

---

## Definition of Done — Stage 4

- [ ] Session mới tự đặt tên có nghĩa sau lượt chat đầu tiên.
- [ ] Bấm Dừng giữa chừng → stream dừng, phần đã sinh vẫn được lưu, không có exception trong log.
- [ ] Ngắt mạng AI provider → FE hiện banner lỗi + nút Thử lại; DB không lưu chuỗi exception.
- [ ] Markdown render đúng và đã qua DOMPurify (test bằng tin nhắn chứa `<img onerror=...>`).
- [ ] Mất kết nối SignalR → tự reconnect, badge cập nhật đúng.
- [ ] Kiểm tra trên màn hình mobile width 375px không vỡ layout.
