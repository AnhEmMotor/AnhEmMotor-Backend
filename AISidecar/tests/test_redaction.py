from app.core import redaction


def test_field_nhay_cam_bi_che():
    result = redaction.redact_dict({"passwordHash": "abc123", "customerName": "Nguyễn Văn A", "total": 100})
    assert result["passwordHash"] == "***"
    assert result["customerName"] == "***"
    assert result["total"] == 100


def test_scrub_text_bat_duoc_email_va_sdt():
    text = "Liên hệ a@b.com hoặc 0912345678 để biết thêm"
    scrubbed = redaction._scrub_text(text)
    assert "a@b.com" not in scrubbed
    assert "0912345678" not in scrubbed
    assert "[email]" in scrubbed
    assert "[số điện thoại]" in scrubbed


def test_redact_dict_de_quy_vao_list_long_nhau():
    payload = {"items": [{"customerName": "A", "total": 100}, {"customerName": "B", "total": 200}]}
    result = redaction.redact_dict(payload)
    assert result["items"][0]["customerName"] == "***"
    assert result["items"][0]["total"] == 100


def test_make_tool_preview_che_field_nhay_cam():
    preview = redaction.make_tool_preview({"passwordHash": "x", "total": 1})
    assert "x" not in preview["preview"]
    assert "***" in preview["preview"]


def test_make_tool_preview_cat_bot_khi_qua_dai():
    preview = redaction.make_tool_preview({"items": [{"note": "a" * 1000}]})
    assert "đã rút gọn" in preview["preview"]
