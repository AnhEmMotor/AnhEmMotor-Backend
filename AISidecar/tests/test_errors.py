from app.core.errors import SidecarError, BackendError, ForbiddenError, LlmError


def test_moi_exception_co_user_message():
    for cls in (SidecarError, BackendError, ForbiddenError, LlmError):
        assert hasattr(cls, "user_message")
        assert cls.user_message


def test_user_message_khong_lo_url_noi_bo():
    err = BackendError("/internal/chat/context", 500)
    for leak in ("http://", "localhost", "127.0.0.1", "/internal/"):
        assert leak not in err.user_message


def test_user_message_khong_lo_str_e():
    err = BackendError("/internal/chat/context", 500)
    assert str(err) not in err.user_message
    assert "/internal/" not in err.user_message


def test_forbidden_error_user_message():
    err = ForbiddenError("/internal/chat/tools/products")
    assert "quyền" in err.user_message.lower() or "không có quyền" in err.user_message.lower()
    assert "/internal/" not in err.user_message


def test_llm_error_user_message():
    err = LlmError()
    assert "AI" in err.user_message
    assert "localhost" not in err.user_message
