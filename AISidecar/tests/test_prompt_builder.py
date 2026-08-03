import pytest
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage

from app.prompts.loader import _read
from app.services.prompt_builder import (
    build_system_message,
    build_history_messages,
    build_store_system_message,
    _read_store_faq,
    FALLBACK_SYSTEM_PROMPT,
)


@pytest.fixture(autouse=True)
def _clear_prompt_cache():
    _read.cache_clear()
    _read_store_faq.cache_clear()
    yield
    _read.cache_clear()
    _read_store_faq.cache_clear()


class TestBuildSystemMessage:
    def test_none_context_tra_fallback(self):
        msg = build_system_message(None)
        assert isinstance(msg, SystemMessage)
        assert msg.content == FALLBACK_SYSTEM_PROMPT
        assert "{" not in msg.content

    def test_empty_dict_context_tra_fallback(self):
        msg = build_system_message({})
        assert msg.content == FALLBACK_SYSTEM_PROMPT

    def test_co_context_nhung_ten(self):
        ctx = {
            "user": {"fullName": "Nguyễn Văn A", "userName": "nguyenvana"},
            "roles": ["Admin"],
            "permissions": ["Products.View"],
        }
        msg = build_system_message(ctx)
        assert "Nguyễn Văn A" in msg.content
        assert "AnhEmMotor" in msg.content

    def test_khong_co_fullname_dung_mac_dinh(self):
        ctx = {"user": {"userName": "someone"}}
        msg = build_system_message(ctx)
        assert "(không rõ)" in msg.content

    def test_khong_co_placeholder_con_sot(self):
        ctx = {"user": {"fullName": "Test"}}
        msg = build_system_message(ctx)
        assert "{" not in msg.content

    def test_server_date_duoc_dua_vao_prompt(self):
        ctx = {"user": {"fullName": "Test"}}
        msg = build_system_message(ctx, "2026-07-30T09:00:00+07:00")
        assert "2026-07-30T09:00:00+07:00" in msg.content

    def test_khong_co_server_date_dung_mac_dinh(self):
        ctx = {"user": {"fullName": "Test"}}
        msg = build_system_message(ctx)
        assert "(không rõ)" in msg.content


class TestBuildStoreSystemMessage:
    def test_faq_content_co_mat(self):
        msg = build_store_system_message()
        assert "Bảo hành" in msg.content
        assert "08:00 - 20:00" in msg.content

    def test_store_faq_khong_bia_chinh_sach_doi_tra(self):
        assert "đổi trả" not in _read_store_faq().lower()

    def test_khong_co_placeholder_con_sot(self):
        msg = build_store_system_message("2026-07-30T09:00:00+07:00")
        assert "{" not in msg.content


class TestBuildHistoryMessages:
    def test_empty_history(self):
        assert build_history_messages(None, "hi") == []
        assert build_history_messages({}, "hi") == []
        assert build_history_messages({"history": []}, "hi") == []

    def test_map_role_user_thanh_human(self):
        ctx = {"history": [{"role": "User", "message": "xin chào"}]}
        msgs = build_history_messages(ctx, "câu mới")
        assert len(msgs) == 1
        assert isinstance(msgs[0], HumanMessage)
        assert msgs[0].content == "xin chào"

    def test_map_role_ai_thanh_ai_message(self):
        ctx = {"history": [{"role": "AI", "message": "chào bạn"}]}
        msgs = build_history_messages(ctx, "câu mới")
        assert len(msgs) == 1
        assert isinstance(msgs[0], AIMessage)

    def test_map_role_assistant_thanh_ai_message(self):
        ctx = {"history": [{"role": "Assistant", "message": "ok"}]}
        msgs = build_history_messages(ctx, "câu mới")
        assert isinstance(msgs[0], AIMessage)

    def test_bo_message_rong(self):
        ctx = {"history": [
            {"role": "User", "message": ""},
            {"role": "User", "message": "thật"},
        ]}
        msgs = build_history_messages(ctx, "câu mới")
        assert len(msgs) == 1
        assert msgs[0].content == "thật"

    def test_bo_trung_cau_hoi_hien_tai(self):
        current = "Xe SH giá bao nhiêu?"
        ctx = {"history": [
            {"role": "AI", "message": "Chào bạn!"},
            {"role": "User", "message": current},
        ]}
        msgs = build_history_messages(ctx, current)
        assert len(msgs) == 1
        assert isinstance(msgs[0], AIMessage)

    def test_khong_bo_khi_cuoi_la_ai(self):
        ctx = {"history": [
            {"role": "User", "message": "hỏi gì đó"},
            {"role": "AI", "message": "trả lời"},
        ]}
        msgs = build_history_messages(ctx, "câu mới")
        assert len(msgs) == 2

    def test_nhieu_luot_giu_dung_thu_tu(self):
        ctx = {"history": [
            {"role": "User", "message": "câu 1"},
            {"role": "AI", "message": "trả lời 1"},
            {"role": "User", "message": "câu 2"},
            {"role": "AI", "message": "trả lời 2"},
        ]}
        msgs = build_history_messages(ctx, "câu 3")
        assert len(msgs) == 4
        assert isinstance(msgs[0], HumanMessage)
        assert isinstance(msgs[1], AIMessage)
        assert isinstance(msgs[2], HumanMessage)
        assert isinstance(msgs[3], AIMessage)

    def test_role_la_khong_map_duoc(self):
        ctx = {"history": [{"role": "system", "message": "bỏ qua"}]}
        msgs = build_history_messages(ctx, "câu mới")
        assert len(msgs) == 0
