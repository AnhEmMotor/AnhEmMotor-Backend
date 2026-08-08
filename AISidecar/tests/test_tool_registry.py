from itertools import combinations

from app.tools import registry


def test_khong_module_nao_vuot_tran():
    specs = registry.load_tool_specs()
    counts = {}
    for spec in specs.values():
        counts[spec.module] = counts.get(spec.module, 0) + 1
    offenders = {m: c for m, c in counts.items() if c > registry.MAX_TOOLS_PER_MODULE}
    assert not offenders


def test_moi_tool_thuoc_dung_mot_module():
    specs = registry.load_tool_specs()
    for spec in specs.values():
        assert spec.module


def test_hai_module_bat_ky_khong_vuot_tran_request():
    specs = registry.load_tool_specs()
    counts = {}
    for spec in specs.values():
        counts[spec.module] = counts.get(spec.module, 0) + 1
    for a, b in combinations(counts, 2):
        assert counts[a] + counts[b] <= registry.MAX_TOOLS_PER_REQUEST


def _spec(name, module, perms=(), **kwargs):
    return registry.ToolSpec(name=name, module=module, required_permissions=tuple(perms), **kwargs)


def test_registry_fingerprint_tat_dinh():
    specs = {"a": _spec("a", "product", ["p1"])}
    assert registry.registry_fingerprint(specs) == registry.registry_fingerprint(specs)


def test_registry_fingerprint_doi_khi_doi_version():
    base = {"a": _spec("a", "product", ["p1"], version=1)}
    bumped = {"a": _spec("a", "product", ["p1"], version=2)}
    assert registry.registry_fingerprint(base) != registry.registry_fingerprint(bumped)


def test_registry_fingerprint_khong_doi_khi_tool_removed_bi_bo_qua():
    active_only = {"a": _spec("a", "product", ["p1"])}
    with_removed = {
        "a": _spec("a", "product", ["p1"]),
        "b": _spec("b", "sales", status="removed"),
    }
    assert registry.registry_fingerprint(active_only) == registry.registry_fingerprint(with_removed)


def test_filter_by_permission_bo_qua_tool_khong_active():
    specs = {
        "a": _spec("a", "product", ["p1"]),
        "b": _spec("b", "product", ["p1"], status="deprecated"),
        "c": _spec("c", "product", ["p1"], status="removed"),
    }
    allowed = registry.filter_by_permission(specs, ["p1"])
    assert {s.name for s in allowed} == {"a"}


def test_filter_by_permission_chi_tra_tool_du_quyen():
    specs = {
        "a": _spec("a", "product", ["p1"]),
        "b": _spec("b", "sales", ["p2"]),
    }
    allowed = registry.filter_by_permission(specs, ["p1"])
    assert {s.name for s in allowed} == {"a"}


def test_select_tools_for_request_ap_tran_va_tat_dinh():
    allowed = [_spec(f"t{i}", "product") for i in range(25)]
    scoped = registry.select_tools_for_request(allowed, ["product"])
    assert len(scoped) == registry.MAX_TOOLS_PER_REQUEST
    scoped_again = registry.select_tools_for_request(allowed, ["product"])
    assert [s.name for s in scoped] == [s.name for s in scoped_again]


def test_select_tools_for_request_uu_tien_router_truoc_module_bo_sung():
    allowed = [_spec("a", "product"), _spec("b", "sales")]
    scoped = registry.select_tools_for_request(allowed, ["product"], expanded_modules={"sales"})
    assert {s.name for s in scoped} == {"a", "b"}


def test_build_tool_scope_gioi_han_theo_plan_step():
    state = {
        "permissions": ["Permissions.Warehouse.ProductManagement.View"],
        "current_plan_step": {"expectedTools": ["get_low_stock_products"]},
        "scoped_modules": [],
        "expanded_modules": set(),
    }
    scoped = registry.build_tool_scope(state)
    assert {s.name for s in scoped} == {"get_low_stock_products"}


async def test_infer_step_tools_loc_ten_bia(monkeypatch):
    class FakeResponse:
        content = '["tool_a", "made_up_tool"]'
        text = content

    class FakeLLM:
        async def ainvoke(self, prompt):
            return FakeResponse()

    monkeypatch.setattr(registry, "get_llm", lambda **kwargs: FakeLLM())
    allowed = [_spec("tool_a", "product"), _spec("tool_b", "product")]
    result = await registry.infer_step_tools("Lấy dữ liệu abcxyz", allowed)
    assert result == ["tool_a"]


async def test_infer_step_tools_khong_crash_khi_gemini_tra_content_dang_list(monkeypatch):
    from langchain_core.messages import AIMessage

    class FakeLLM:
        async def ainvoke(self, prompt):
            return AIMessage(content=[{"type": "text", "text": '["tool_a"]'}])

    monkeypatch.setattr(registry, "get_llm", lambda **kwargs: FakeLLM())
    allowed = [_spec("tool_a", "product"), _spec("tool_b", "product")]
    result = await registry.infer_step_tools("Lấy dữ liệu abcxyz", allowed)
    assert result == ["tool_a"]
