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
