from app.services import plan_cache


def test_intent_hash_thang_khac_nhau_cho_cung_hash():
    h1 = plan_cache.intent_hash("doanh thu tháng 7", "sales")
    h2 = plan_cache.intent_hash("doanh thu tháng 6", "sales")
    assert h1 == h2


def test_intent_hash_module_khac_nhau_cho_hash_khac():
    h1 = plan_cache.intent_hash("báo cáo tồn kho", "inventory")
    h2 = plan_cache.intent_hash("báo cáo tồn kho", "sales")
    assert h1 != h2


def test_intent_hash_bo_qua_tu_dem():
    h1 = plan_cache.intent_hash("cho tôi xem doanh thu tháng này", "sales")
    h2 = plan_cache.intent_hash("doanh thu tháng này", "sales")
    assert h1 == h2


def test_intent_hash_on_dinh_qua_nhieu_lan_goi():
    assert plan_cache.intent_hash("doanh thu tháng 7", "sales") == plan_cache.intent_hash(
        "Doanh Thu Tháng 7?", "sales")


def test_contains_hardcoded_data_phat_hien_ma_don():
    steps = [{"title": "Lấy đơn hàng DH-2026-001", "detail": "Gọi get_order_status"}]
    assert plan_cache.contains_hardcoded_data(steps)


def test_contains_hardcoded_data_phat_hien_so_dien_thoai():
    steps = [{"title": "Tra khách hàng", "detail": "Số điện thoại 0912345678"}]
    assert plan_cache.contains_hardcoded_data(steps)


def test_contains_hardcoded_data_phat_hien_so_tien_cu_the():
    steps = [{"title": "Kiểm tra công nợ", "detail": "Khách nợ 98.000.000 đồng"}]
    assert plan_cache.contains_hardcoded_data(steps)


def test_contains_hardcoded_data_khong_bao_dong_gia_voi_placeholder():
    steps = [{"title": "Báo cáo {{from_date}} đến {{to_date}}", "detail": "Gọi get_sales_summary"}]
    assert not plan_cache.contains_hardcoded_data(steps)


def test_render_steps_thay_placeholder_dung_slot():
    steps = [{"id": "s1", "order": 1, "title": "Báo cáo {{from_date}} đến {{to_date}}",
              "detail": "Gọi tool từ {{from_date}}"}]
    rendered = plan_cache.render_steps(steps, {"from_date": "2026-07-01", "to_date": "2026-07-31"})
    assert rendered[0]["title"] == "Báo cáo 2026-07-01 đến 2026-07-31"
    assert rendered[0]["detail"] == "Gọi tool từ 2026-07-01"


async def test_fill_slots_rong_khi_khong_co_slot():
    result = await plan_cache.fill_slots([], "doanh thu tháng này", "2026-08-04")
    assert result == {}


async def test_fill_slots_goi_llm_voi_max_tokens_thap(monkeypatch):
    captured = {}

    class FakeStructured:
        async def ainvoke(self, prompt):
            captured["prompt"] = prompt

            class Result:
                def model_dump(self):
                    return {"from_date": "2026-07-01", "to_date": "2026-07-31"}
            return Result()

    class FakeLLM:
        def with_structured_output(self, schema):
            return FakeStructured()

    def fake_get_llm(temperature=0.1, max_output_tokens=None):
        captured["max_output_tokens"] = max_output_tokens
        return FakeLLM()

    monkeypatch.setattr(plan_cache, "get_llm", fake_get_llm)

    slots = [{"name": "from_date", "type": "date", "description": "Ngày bắt đầu"},
              {"name": "to_date", "type": "date", "description": "Ngày kết thúc"}]
    result = await plan_cache.fill_slots(slots, "doanh thu tháng 7", "2026-08-04")

    assert result == {"from_date": "2026-07-01", "to_date": "2026-07-31"}
    assert captured["max_output_tokens"] == 200
    assert "2026-08-04" in captured["prompt"]
