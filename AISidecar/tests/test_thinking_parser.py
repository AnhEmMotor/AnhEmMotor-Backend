from app.agents.manager_agent import ThinkingParser


def test_khong_co_tag_thi_passthrough_nguyen_ven():
    parser = ThinkingParser()
    out = "".join(parser.feed(chunk) for chunk in ["Xin chào, ", "doanh thu tháng này là 0."])
    assert out == "Xin chào, doanh thu tháng này là 0."
    assert parser.thinking_text == ""


def test_tag_nguyen_khoi_trong_mot_lan_feed():
    parser = ThinkingParser()
    out = parser.feed("<suy_nghi>Cần tra doanh thu.</suy_nghi>Doanh thu tháng này là 0.")
    assert out == "Doanh thu tháng này là 0."
    assert parser.thinking_text == "Cần tra doanh thu."


def test_tag_chia_nho_qua_nhieu_chunk_ke_ca_cat_giua_the_mo():
    parser = ThinkingParser()
    chunks = ["<suy", "_nghi>", "Cần gọi tool ", "get_sales_summary.", "</suy_nghi>", "Doanh thu 0 đồng."]
    visible = "".join(parser.feed(c) for c in chunks)
    assert visible == "Doanh thu 0 đồng."
    assert parser.thinking_text == "Cần gọi tool get_sales_summary."


def test_the_dong_chia_nho_qua_nhieu_chunk():
    parser = ThinkingParser()
    chunks = ["<suy_nghi>nội dung</suy", "_nghi>", "phần còn lại"]
    visible = "".join(parser.feed(c) for c in chunks)
    assert visible == "phần còn lại"
    assert parser.thinking_text == "nội dung"


def test_stream_bi_ngat_giua_luc_dang_trong_the_khong_lo_ky_tu_nao():
    parser = ThinkingParser()
    visible = "".join(parser.feed(c) for c in ["<suy_nghi>", "tên sản phẩm bịa Honda XYZ-999"])
    assert visible == ""
    assert parser.thinking_text == ""


def test_chuoi_gan_giong_the_nhung_khong_khop_khong_bi_nuot():
    parser = ThinkingParser()
    out = "".join(parser.feed(c) for c in ["<suy nghĩ của tôi là...", " tiếp tục câu trả lời."])
    assert out == "<suy nghĩ của tôi là... tiếp tục câu trả lời."
    assert parser.thinking_text == ""


def test_noi_dung_sau_khi_dong_the_van_duoc_stream_tiep_tung_chunk():
    parser = ThinkingParser()
    assert parser.feed("<suy_nghi>abc</suy_nghi>") == ""
    assert parser.feed("phần 1 ") == "phần 1 "
    assert parser.feed("phần 2") == "phần 2"
