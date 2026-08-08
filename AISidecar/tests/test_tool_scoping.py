from app.agents import manager_agent
from app.services import routing
from app.tools import registry


def test_follow_up_tai_dung_module_luot_truoc():
    ctx = {"lastModules": ["product"], "entities": {"product": "SH 150i"}}
    assert routing.is_follow_up("còn màu đen không?", ctx)


async def test_resolve_modules_fast_path_khong_goi_router(monkeypatch):
    async def _boom(*args, **kwargs):
        raise AssertionError("không nên gọi router khi có fast path")

    monkeypatch.setattr(routing, "route_question", _boom)
    ctx = {"lastModules": ["product"], "entities": {"product": "SH 150i"}}
    result = await routing.resolve_modules("còn màu đen không?", ctx, [])
    assert result == ["product"]


def test_khong_co_ngu_canh_thi_khong_fast_path():
    assert not routing.is_follow_up("còn màu đen không?", {})


def test_cau_dai_khong_fast_path():
    ctx = {"lastModules": ["product"]}
    long_q = "cho tôi xem doanh thu tháng này so với cùng kỳ năm ngoái theo từng danh mục"
    assert not routing.is_follow_up(long_q, ctx)


def test_digest_khong_phinh_theo_lich_su():
    history = [{"role": "User", "message": f"câu hỏi số {i}"} for i in range(200)]
    digest = routing.build_routing_digest(history, {})
    assert len(digest) < 800
    assert "câu hỏi số 199" in digest
    assert "câu hỏi số 100" not in digest


def test_digest_khong_chua_cau_tra_loi_cua_ai():
    history = [
        {"role": "User", "message": "doanh thu?"},
        {"role": "AI", "message": "Doanh thu tháng 7 đạt 1,24 tỷ đồng " * 50},
    ]
    assert "1,24 tỷ" not in routing.build_routing_digest(history, {})


async def test_steering_queue_mo_rong_scope(monkeypatch):
    async def _fake_route(query, digest):
        return ["inventory"]

    monkeypatch.setattr(routing, "route_question", _fake_route)

    state = {
        "carried_steering": [{"content": "thêm cả tồn kho nữa", "mode": "queue"}],
        "auth_header": "Bearer x",
        "run_id": "r1",
        "permissions": [],
        "scoped_modules": ["sales"],
        "expanded_modules": set(),
        "routing_context": {},
        "history": [],
    }
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    result = await manager_agent.absorb_steering_node(state)
    assert "sales" in result["scoped_modules"]
    assert "inventory" in result["scoped_modules"]


async def test_steering_interrupt_thay_the_scope(monkeypatch):
    async def _fake_route(query, digest):
        return ["hr"]

    monkeypatch.setattr(routing, "route_question", _fake_route)

    state = {
        "carried_steering": [{"content": "à nhầm, tôi hỏi về nhân sự", "mode": "interrupt"}],
        "auth_header": "Bearer x",
        "run_id": "r1",
        "permissions": [],
        "scoped_modules": ["sales"],
        "expanded_modules": set(),
        "routing_context": {},
        "history": [],
    }
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    result = await manager_agent.absorb_steering_node(state)
    assert "sales" not in result["scoped_modules"]
    assert "hr" in result["scoped_modules"]


def test_plan_step_gioi_han_scope(monkeypatch):
    allowed_spec = registry.ToolSpec(name="get_low_stock_products", module="inventory")
    pinned_spec = registry.ToolSpec(name="some_pinned_tool", module="knowledge")
    state = {
        "current_plan_step": {"expectedTools": ["get_low_stock_products"]},
        "scoped_modules": [], "expanded_modules": set(),
        "permissions": [],
    }
    monkeypatched = {allowed_spec.name: allowed_spec, pinned_spec.name: pinned_spec}
    monkeypatch.setattr(registry, "load_tool_specs", lambda: monkeypatched)
    monkeypatch.setattr(registry, "PINNED_TOOLS", frozenset({"some_pinned_tool"}))
    scope = registry.build_tool_scope(state)
    assert {t.name for t in scope} == {"get_low_stock_products", "some_pinned_tool"}


async def test_infer_step_tools_loc_ten_bia(monkeypatch):
    class FakeResponse:
        content = '["tool_a", "made_up_tool"]'
        text = content

    class FakeLLM:
        async def ainvoke(self, prompt):
            return FakeResponse()

    monkeypatch.setattr(registry, "get_llm", lambda **kwargs: FakeLLM())
    tool_a = registry.ToolSpec(name="tool_a", module="product")
    tool_b = registry.ToolSpec(name="tool_b", module="product")
    result = await registry.infer_step_tools("Lấy dữ liệu abcxyz", [tool_a, tool_b])
    assert all(n in {"tool_a", "tool_b"} for n in result)


def test_routing_context_khong_chua_so_lieu():
    ctx = routing.extract_entities([{"args": {"product_id": "p1", "total_revenue": 1_240_000_000}}])
    assert "product" in ctx
    assert not any("revenue" in k or "total" in k for k in ctx)


def test_routing_context_het_hieu_luc_sau_30_phut():
    old = {"lastModules": ["product"], "updatedAt": "2026-07-26T06:00:00+07:00"}
    expired = routing.expire_if_stale(old, now="2026-07-26T09:00:00+07:00")
    assert not routing.is_follow_up("còn màu đen không?", expired)
