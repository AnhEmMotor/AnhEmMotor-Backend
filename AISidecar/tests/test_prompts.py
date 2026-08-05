import pytest
from app.prompts.loader import render, _read


@pytest.fixture(autouse=True)
def _clear_prompt_cache():
    _read.cache_clear()
    yield
    _read.cache_clear()


def test_render_thay_placeholder():
    result = render("title_generation", message="xin chào")
    assert "xin chào" in result


def test_render_system_manager_chat():
    result = render("system_manager_chat", full_name="Nguyễn Văn A", server_date="2026-07-30T09:00:00+07:00")
    assert "AnhEmMotor" in result
    assert "Nguyễn Văn A" in result
    assert "2026-07-30T09:00:00+07:00" in result


def test_thieu_file_raise_file_not_found():
    with pytest.raises(FileNotFoundError, match="khong_ton_tai"):
        render("khong_ton_tai")


def test_thieu_bien_raise_key_error():
    with pytest.raises(KeyError):
        render("title_generation")
