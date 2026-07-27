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


def test_render_khong_co_placeholder():
    result = render("system_manager_chat")
    assert "AnhEmMotor" in result


def test_thieu_file_raise_file_not_found():
    with pytest.raises(FileNotFoundError, match="khong_ton_tai"):
        render("khong_ton_tai")


def test_thieu_bien_raise_key_error():
    with pytest.raises(KeyError):
        render("title_generation")
