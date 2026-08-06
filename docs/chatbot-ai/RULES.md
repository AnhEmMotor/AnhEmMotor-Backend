# Quy ước vòng đời Tool

> Nguồn: [17-STAGE-TOOL-LIFECYCLE.md](done/17-STAGE-TOOL-LIFECYCLE.md) mục 17.2, 17.11.

## Quy ước tăng `version`

| Loại thay đổi | Tăng version? | Cần làm |
|---|---|---|
| Sửa mô tả tool | Không | — |
| Thêm tham số **tuỳ chọn** | Không | — |
| Thêm tham số **bắt buộc** | **Có** | Tool cũ thành `deprecated` |
| Đổi tên / đổi kiểu tham số | **Có** | Như trên |
| Đổi ý nghĩa field trả về | **Có** | Cập nhật `GLOSSARY.md` |
| Gỡ tool | — | `status = "removed"`, đặt `replaced_by` |

`registry_fingerprint()` (`AISidecar/app/tools/registry.py`) hash `(name, version, required_permissions)`
của mọi tool `status == "active"`. Đổi version hoặc quyền → đổi hash; sửa mô tả → không đổi.

## Chính sách khai tử tool

```
Thêm tool mới  → status = active
Đổi schema     → tool mới version+1 (active) + tool cũ deprecated, chạy song song ≥ 2 tuần
Gỡ tool        → deprecated ≥ 2 tuần → removed (giữ ToolSpec với replaced_by ≥ 3 tháng)
Xoá ToolSpec   → chỉ sau 3 tháng ở trạng thái removed
```

Tool `deprecated` vẫn hoạt động nhưng không đưa vào registry của user mới; thêm tiền tố `[SẮP NGỪNG]`
vào mô tả để model ưu tiên tool thay thế.

**Giữ `ToolSpec` với `status = "removed"`** là điểm quan trọng nhất — nhờ đó `resolve_tool_call_error`
trả được thông báo "tool này đã bị gỡ, hãy dùng X" (`replaced_by`) thay vì "không tìm thấy tool" vô
nghĩa với model.

## Sửa catalog ở đâu

Một file duy nhất `SharedConfig/chat-tools-catalog.json` là nguồn thật cho **cả** sidecar (Python,
`app/tools/registry.py`) lẫn backend (C#, `IChatToolCatalogProvider`) — sửa 1 chỗ, cả hai phía đọc lại.
Thêm tool mới hoặc đổi field: chỉ sửa file JSON này, không cần đồng bộ tay hai nơi.

---

# Nguồn sự thật cho từng loại dữ liệu

> Nguồn: [18-STAGE-CONSISTENCY.md](18-STAGE-CONSISTENCY.md) mục 18.1.

| Dữ liệu | Nguồn sự thật | Hai lớp kia làm gì |
|---|---|---|
| Lịch sử hội thoại | `ChatMessage` (.NET) | FE cache để hiển thị; sidecar nhận bản sạch hoá (Stage 17.4) |
| Trạng thái run | `ChatRun.Status` (.NET) | FE suy ra từ event; sidecar **không** quyết định |
| Nội dung đang stream | `ChatRunEvent` (.NET) | FE dựng lại từ `Seq`; sidecar chỉ phát ra |
| Plan | `ChatPlan` (.NET) | FE giữ `version` để phát hiện lệch; sidecar đọc lại mỗi bước |
| State suy luận của agent | checkpointer (sidecar) | .NET không cần biết nội dung, chỉ cần biết còn/hết |
| Permission | DB (.NET) | Sidecar chỉ nhận bản chụp theo run |

**Quy tắc bao trùm: .NET là nguồn sự thật cho mọi thứ người dùng nhìn thấy.**
Sidecar là bộ máy tính toán không có ký ức lâu dài — mất state của nó thì run làm lại được,
không mất dữ liệu người dùng.

## Hai đường dữ liệu tách biệt (Stage 18.11)

```
Kết quả tool
   ├─→ [đường 1] LLM        : dữ liệu ĐẦY ĐỦ, không redact
   └─→ [đường 2] ChatRunEvent → FE : ĐÃ redact (Stage 11)
```

`make_tool_preview()` chỉ được gọi khi phát event cho FE (`manager_agent.py` — `tool_start`,
`tool_end`). Tuyệt đối không gọi nó trên dữ liệu đưa vào `ToolMessage` của LLM
(`AISidecar/tests/test_consistency.py::test_du_lieu_vao_llm_khong_bi_che_con_fe_thi_co`).
