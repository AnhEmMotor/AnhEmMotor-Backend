# Chatbot AI — Kế hoạch hoàn thiện (Overview)

> Branch: `thanhbinh/feat/create-chatbot-ai`
> Cập nhật: 2026-07-26

Mục lục cho toàn bộ kế hoạch hoàn thiện Chatbot AI (Manager Chat).
Mỗi Stage là một file riêng, mỗi Stage nên là một PR.

> ⚠️ **Số hiệu file là ID, không phải thứ tự thực hiện.** Xem thứ tự đề xuất ở mục 4.

---

## 1. Kiến trúc hiện tại

```
┌─────────────────────────┐
│ AnhEmMotor-Management   │  ChatDrawer.vue (641 dòng)
│ (Vue 3 + SignalR)       │  chat.api.ts
└───────────┬─────────────┘
            │ SignalR stream · REST /api/v1/manager-chat/...
┌───────────▼─────────────┐
│ AnhEmMotor-Backend      │  ManagerChatHub.cs · ManagerChatController.cs
│ (.NET 8, CQRS/MediatR)  │  Features/ManagerChat/{Commands,Queries}
│                         │  Domain: ChatSession, ChatMessage
│                         │  AiSidecarManager (spawn process Python)
└───────────┬─────────────┘
            │ HTTP POST /manager-chat
            │ ◄── POST /internal/chat/context (user, roles, permissions)
┌───────────▼─────────────┐
│ AISidecar               │  main.py · llm_factory.py
│ (Python FastAPI +       │  controllers/manager_chat_controller.py
│  LangChain)             │  controllers/search_controller.py
└───────────┬─────────────┘
            │
      Gemini (gemini-3.5-flash) / OpenAI-compatible endpoint
```

### Kiến trúc đích (sau khi hoàn tất)

```
Vue ──SignalR── .NET Run Engine ──HTTP── AISidecar (LangGraph agent)
                     │                          ├── 71 tool → /internal/chat/tools/*
                     │                          ├── Qdrant (semantic search + RAG)
                     │                          └── Guardrails (permission, injection, loop)
                     ├── ChatRun / ChatRunEvent  (chạy nền, tua lại, khôi phục)
                     └── ChatPlan                (plan mode, user sửa được)
```

### File chính hiện có

| Lớp | File |
|---|---|
| FE Drawer | `AnhEmMotor-Management/src/components/business/chat/ChatDrawer.vue` |
| FE API | `AnhEmMotor-Management/src/api/chat/chat.api.ts` |
| Hub | `WebAPI/Hubs/ManagerChatHub.cs` |
| Controller | `WebAPI/Controllers/V1/ManagerChatController.cs` |
| Internal ctx | `WebAPI/Controllers/InternalChatController.cs` |
| Stream handler | `Application/Features/ManagerChat/Commands/StreamManagerChatMessage/` |
| Entity | `Domain/Entities/ChatSession.cs`, `ChatMessage.cs` |
| Repository | `Infrastructure/Repositories/Chat/` |
| Sidecar mgr | `Infrastructure/Services/Ai/AiSidecarManager.cs` |
| Sidecar | `AISidecar/` |
| Config | `WebAPI/appsettings.json` → `AISetup` |
| Phân quyền | `Domain/Constants/Permission/` — 6 module, 185 hằng số |

---

## 2. Trạng thái hiện tại

### Đã có
- CRUD phiên chat + lịch sử (CQRS đầy đủ, migration MySQL + PostgreSQL).
- Streaming từ Gemini về FE qua SignalR.
- Sidecar tự spawn bởi `AiSidecarManager`.
- `/internal/chat/context` trả user + roles + permissions, có `[LocalhostOnly]`.
- AI Search: `search_controller.py` bóc ý định tìm kiếm bằng `PydanticOutputParser`.

### Chưa có / đang lỗi
| # | Vấn đề | Stage |
|---|---|---|
| 1 | `SendMessage` (REST) parse field `reply` nhưng sidecar trả `text/plain` → luôn lỗi | 01 |
| 2 | `/manager-chat` không verify internal secret; sidecar bind `0.0.0.0` | 01 |
| 3 | Lệch role type: TS khai `Assistant`, BE lưu `AI` | 01 |
| 4 | Context fetch về nhưng **không nhúng** vào prompt | 02 |
| 5 | Không có trí nhớ hội thoại | 02 |
| 6 | Không có tool calling → AI không truy vấn được dữ liệu thật | 03, 15 |
| 7 | `generate-title` chỉ cắt 30 ký tự; FE chưa gọi | 04 |
| 8 | Chưa scope quyền, chưa chống injection, chưa rate limit | 05, 13 |
| 9 | Chưa có test cho luồng chat, chưa bật observability | 06 |
| 10 | Sidecar không có cấu trúc, config rải rác, `except: pass` | 07 |
| 11 | Đóng tab giữa chừng → mất câu trả lời đang sinh | 08 |
| 12 | Không gửi tiếp được khi AI đang chạy | 09 |
| 13 | Không có plan mode | 10 |
| 14 | Không xem được quá trình suy nghĩ / kết quả tool | 11 |
| 15 | Không có tìm kiếm ngữ nghĩa / RAG | 12 |
| 16 | Chưa tối ưu tốc độ và số vòng suy nghĩ | 14 |

---

## 3. Danh sách Stage

| ID | Tên | File | Yêu cầu | Ước lượng |
|---|---|---|---|---|
| 01 | Sửa nền móng & dọn bug | [01-STAGE-FOUNDATION-FIXES.md](01-STAGE-FOUNDATION-FIXES.md) | — | 0.5–1 ngày |
| 02 | Context + Trí nhớ hội thoại | [02-STAGE-CONTEXT-MEMORY.md](02-STAGE-CONTEXT-MEMORY.md) | — | 1–2 ngày |
| 03 | Tool Calling (hạ tầng + 6 tool mẫu) | [03-STAGE-TOOL-CALLING.md](03-STAGE-TOOL-CALLING.md) | — | 3–5 ngày |
| 04 | Hoàn thiện UX & Frontend | [04-STAGE-UX-FRONTEND.md](04-STAGE-UX-FRONTEND.md) | — | 2–3 ngày |
| 05 | Bảo mật & Giới hạn | [05-STAGE-SECURITY.md](05-STAGE-SECURITY.md) | — | 1–2 ngày |
| 06 | Testing, Observability & Deploy | [06-STAGE-TESTING-OBSERVABILITY.md](06-STAGE-TESTING-OBSERVABILITY.md) | — | 2–3 ngày |
| 07 | Tái cấu trúc AI Sidecar | [07-STAGE-SIDECAR-ARCHITECTURE.md](07-STAGE-SIDECAR-ARCHITECTURE.md) | **#7** | 1–2 ngày |
| 08 | Run Engine: chạy nền & khôi phục | [08-STAGE-RUN-ENGINE.md](08-STAGE-RUN-ENGINE.md) | **#4** | 3–4 ngày |
| 09 | Steering: chat tiếp khi đang suy nghĩ | [09-STAGE-STEERING.md](09-STAGE-STEERING.md) | **#2** | 2–3 ngày |
| 10 | Plan Mode: tạo & sửa kế hoạch | [10-STAGE-PLAN-MODE.md](10-STAGE-PLAN-MODE.md) | **#3** | 3–4 ngày |
| 11 | Hiển thị suy nghĩ & kết quả tool | [11-STAGE-REASONING-TRANSPARENCY.md](11-STAGE-REASONING-TRANSPARENCY.md) | **#5** | 2–3 ngày |
| 12 | Qdrant & RAG | [12-STAGE-QDRANT-RAG.md](12-STAGE-QDRANT-RAG.md) | **#1** | 4–5 ngày |
| 13 | Guardrails: không để AI hớ tool | [13-STAGE-GUARDRAILS.md](13-STAGE-GUARDRAILS.md) | **#6** | 2–3 ngày |
| 14 | Tối ưu tốc độ & số lần suy nghĩ | [14-STAGE-PERFORMANCE.md](14-STAGE-PERFORMANCE.md) | **#5** | 2–3 ngày |
| 15 | Danh mục Tool đầy đủ (71 tool) | [15-STAGE-TOOL-CATALOG.md](15-STAGE-TOOL-CATALOG.md) | **bổ sung** | 8–12 ngày |
| 16 | Độ chính xác dữ liệu (chống số sai lệch) | [16-STAGE-TOOL-DATA-FIDELITY.md](16-STAGE-TOOL-DATA-FIDELITY.md) | **bổ sung** | 3–4 ngày |
| 17 | Vòng đời Tool & hợp đồng phiên bản | [17-STAGE-TOOL-LIFECYCLE.md](17-STAGE-TOOL-LIFECYCLE.md) | **bổ sung** | 3–4 ngày |
| 18 | Nhất quán & hoà giải trạng thái | [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) | **bổ sung** | 3–4 ngày |
| 19 | Cache Plan (giảm số lần suy nghĩ) | [19-STAGE-PLAN-CACHE.md](19-STAGE-PLAN-CACHE.md) | **bổ sung** | 3–4 ngày |
| 20 | Chọn tool động theo ngữ cảnh | [20-STAGE-DYNAMIC-TOOL-SCOPING.md](20-STAGE-DYNAMIC-TOOL-SCOPING.md) | **bổ sung** | 3–4 ngày |

**Tổng ước lượng: ~58–76 ngày công.**

---

## 4. Thứ tự thực hiện đề xuất

Số hiệu file là ID cố định. Thứ tự làm việc như sau:

```
Đợt 1 — Nền móng (bắt buộc trước mọi thứ)
  01 Sửa bug  →  07 Tái cấu trúc sidecar  →  02 Context & trí nhớ

Đợt 2 — Kiến trúc chạy nền (nền cho 09, 10, 11, 18)
  08 Run Engine  →  09 Steering  →  18 Consistency

Đợt 3 — Năng lực cốt lõi
  03 Tool Calling  →  13 Guardrails + 20 Tool Scoping  →  17 Tool Lifecycle
  →  16 Data Fidelity  →  15 Tool Catalog (P1)

Đợt 4 — Trải nghiệm nâng cao
  11 Transparency  →  10 Plan Mode  →  12 Qdrant/RAG  →  19 Plan Cache

Đợt 5 — Hoàn thiện
  14 Performance  →  15 Tool Catalog (P2, P3)  →  04 UX  →  05 Security  →  06 Testing
```

### Vì sao thứ tự này

| Quyết định | Lý do |
|---|---|
| **07 ngay sau 01** | Refactor sidecar khi còn 195 dòng tốn 1 ngày; sau khi có agent + 71 tool tốn 1 tuần |
| **08 trước 09/10/11** | Cả ba đều xây trên event log và checkpointer của Run Engine |
| **13 trước 15** | Guardrails là điều kiện để mở rộng lên 71 tool an toàn |
| **16 trước 15** | Envelope, parity test và cờ shadow phải có **trước** khi nhân bản lên 71 tool — nếu không sẽ phải sửa lại cả 71 |
| **17 trước 15/16** | `registry_fingerprint` là nền cho revalidate plan (17.8), cache plan (19.6) và cờ tool (16.8). Và **run token của 17.9 là điều kiện để Plan Mode duyệt sau 24h chạy được** |
| **18 ngay sau 09** | Ba lớp state (FE/.NET/sidecar) vừa hình thành đủ ở Stage 09 là lúc phải chốt nguồn sự thật, trước khi 10/11 xây thêm lên |
| **20 cùng lúc với 13** | 13.3b chọn scope một lần ở đầu run; 20 tính lại ở mỗi bước. Tách ra làm sau nghĩa là viết lại phần chọn tool hai lần |
| **19 sau 10 + 12 + 17** | Cache plan cần Plan Mode để có plan, Qdrant để tra ngữ nghĩa, và fingerprint để vô hiệu hoá đúng lúc |
| **11 trước 10** | Plan Mode cần hạ tầng hiển thị event; làm 11 trước thì 10 nhẹ đi |
| **14 sau 12** | Tối ưu khi đã có đủ thành phần thật để đo |
| **04/05/06 cuối** | Là các Stage rà soát tổng thể — cần hệ thống đã hoàn chỉnh |

> **04, 05, 06 làm cuốn chiếu.** Ví dụ redaction (Stage 11) và permission tool (Stage 13) đều là
> hạng mục bảo mật — làm ngay trong Stage đó, Stage 05 chỉ là lượt rà soát tổng thể cuối cùng.

### Mốc bàn giao được

| Mốc | Sau Stage | Người dùng nhận được |
|---|---|---|
| **M1 — Chạy đúng** | 01, 07, 02 | Chatbot nhớ hội thoại, biết user là ai |
| **M2 — Không mất việc** | 08, 09, 18 | Thoát ra vào lại không mất câu trả lời; gửi tiếp được khi AI đang chạy; ba lớp state không lệch nhau |
| **M3 — Trợ lý thật** | 03, 13, 17, 16, 15-P1 | Trả lời được bằng dữ liệu thật (20 tool), số khớp báo cáo UI, đổi tool không gây lỗi âm ỉ |
| **M4 — Minh bạch & thông minh** | 11, 10, 12, 19 | Xem được AI nghĩ gì; lập & sửa kế hoạch; tìm kiếm ngữ nghĩa; tái dùng kế hoạch quen |
| **M5 — Sẵn sàng production** | 14, 15, 04, 05, 06 | Nhanh, phủ 71 tool, đã kiểm thử & bảo mật |

---

## 5. Quyết định cần chốt trước khi code

| # | Quyết định | Ở Stage | Mặc định trong plan |
|---|---|---|---|
| 1 | Bỏ hay giữ đường REST `SendMessage` | 01.1 | **Bỏ** (Hướng A) |
| 2 | Điền tên permission thật vào bảng tool | 03.2, 15 | Đã điền theo `Domain/Constants/Permission/` |
| 3 | Có làm tóm tắt hội thoại dài không (cần cột `Summary`) | 02.4 | Chưa làm, để Stage 14 |
| 4 | Checkpointer: `MemorySaver` hay `AsyncPostgresSaver` | 08.9, 10.6 | Memory ở 08, **nâng lên Postgres ở 10** |
| 5 | Protocol stream: JSON lines | 03.4, 08.9 | **JSON lines** |
| 6 | Agent: LangGraph hay `AgentExecutor` | 03.4 | **LangGraph** — `create_react_agent` ở Stage 3, `StateGraph` tự dựng từ Stage 9. Không tự chế vòng lặp agent (xem 7.8b) |
| 7 | Embedding model (đổi = reindex toàn bộ) | 12.3 | `text-embedding-004` |
| 8 | Có bật tool ghi dữ liệu không | 13.5, 15-P3 | **Không** ở bản đầu |
| 9 | Có đưa tool lương/hoa hồng vào chatbot không | 15-G4/G5 | **Cân nhắc loại bỏ** |
| 10 | Mức hiển thị tool ở Production | 11.2 | **Full** (mặc định) |
| 11 | **Chốt định nghĩa nghiệp vụ** (doanh thu, số đơn, tồn kho...) với người phụ trách | 16.4 | **Chưa chốt — cần làm trước tool tài chính** |
| 12 | Múi giờ: backend trả `serverDate` GMT+7 hay sidecar tự tính | 16.2 | **Backend trả** — sidecar không tự tính |
| 13 | **Token cho run nền**: run token riêng / refresh token / rút timeout plan | 17.9 | **Run token riêng (phương án A)** — cần chốt trước khi làm Stage 10 |
| 14 | Cache tool: TTL theo thời gian hay theo phạm vi run | 18.2 | **Theo phạm vi run** (`RunSnapshot`) |
| 15 | Cache plan có được tự động duyệt không | 19.5 | **Chỉ khi toàn bộ tool chỉ-đọc** và template đã chạy tốt ≥ 10 lần |
| 16 | Chọn tool: LLM router theo module hay Qdrant tool retrieval | 20.9 | **Router là chính**; Qdrant chỉ làm fail-safe + gợi ý khi bịa tên tool |
| 17 | Trần module khi steering `queue` mở rộng scope | 20.7 | **3 module** (thay vì 2), trần 20 tool vẫn giữ |

---

## 6. Quy ước chung

- **Ngôn ngữ**: comment tiếng Việt, code tiếng Anh — theo repo hiện tại.
- **Model**: `gemini-3.5-flash` — **giữ nguyên**, chỉ nằm ở `appsettings.json` và fallback của `llm_factory.py`.
- **Backend**: giữ đúng CQRS — mỗi thao tác 1 Command/Query + Handler riêng thư mục.
- **Repository**: tách 4 interface `IChatRead/Insert/Update/DeleteRepository`.
- **Sidecar**: business logic AI ở Python; .NET orchestrate + persist. LLM **không bao giờ** chạm DB trực tiếp.
- **Permission**: kiểm tra ở backend .NET, **không** dựa vào prompt.
- **Migration**: luôn tạo cho **cả** MySQL và PostgreSQL.
- **Secret**: đi qua `AISetup` → env của sidecar; không commit.
- **Mỗi Stage 1 PR**, phải pass Definition of Done ở cuối file Stage.

---

## 7. Nghiệm thu tổng thể

- [ ] Hỏi "Tháng này doanh thu bao nhiêu?" → số liệu thật từ DB.
- [ ] Hỏi tiếp "So với tháng trước?" → hiểu ngữ cảnh, không hỏi lại.
- [ ] Không có quyền xem doanh thu → AI từ chối, **không con số nào lọt ra**.
- [ ] Đóng tab giữa lúc AI trả lời → mở lại thấy đúng trạng thái, không mất chữ.
- [ ] Đang chạy vẫn gửi được "à nhầm, tháng trước cơ" → AI đổi hướng.
- [ ] Yêu cầu phức tạp → AI lập plan, sửa được plan **trong lúc đang tạo**, duyệt rồi mới chạy.
- [ ] Xem được AI nghĩ gì và gọi tool nào; Production **không lộ** tham số/kết quả thô.
- [ ] "Xe ga tiết kiệm xăng cho nữ" → gợi ý đúng, giá và tồn kho lấy từ SQL.
- [ ] TTFT < 1.5s (p50); trung vị số vòng agent ≤ 3.
- [ ] 71 tool phủ hết phân hệ; độ chính xác chọn tool ≥ 88%.
- [ ] Hỏi "Xe SH giá bao nhiêu?" → "còn màu đen không?" → hiểu đúng, **không gọi lại router**.
- [ ] Session 200 tin nhắn: độ chính xác chọn tool **không giảm** (digest cố định < 200 token).
- [ ] Số tool trung vị nạp vào mỗi bước **≤ 5**; khi chạy theo plan chỉ 2–3 tool.
- [ ] Số liệu AI đưa ra **khớp chính xác** với báo cáo trên Management UI (parity test xanh).
- [ ] Hỏi "doanh thu hôm nay" lúc 6h sáng → đúng ngày theo giờ Việt Nam, không lệch sang hôm qua.
- [ ] Gỡ một tool → hội thoại cũ và plan cũ không gây lỗi lặp; AI thông báo rõ ràng.
- [ ] **Duyệt plan sau 24 giờ vẫn thực thi được** (run token mới, permission được revalidate).
- [ ] Bấm Dừng → sidecar thực sự dừng gọi LLM (kiểm chứng bằng log token của provider).
- [ ] Hai con số trong cùng một câu trả lời luôn thuộc **cùng một ảnh chụp** dữ liệu.
- [ ] Hỏi lại câu hỏi quen thuộc → cache plan hit, lập plan < 0.5s, vẫn hiện plan để duyệt.
- [ ] Eval bảo mật (permission, injection, bịa số) pass **100%**.
- [ ] Trace đầy đủ trên LangSmith; `/health` báo đỏ khi sidecar chết.
