import json

from app.agents import manager_agent
from app.services.prompt_builder import build_history_messages
from app.tools import registry


class FakeBackendClient:
    def __init__(self, auth_header):
        pass

    async def pull_pending_steering(self, run_id):
        return []

    async def call_tool(self, tool_path, payload):
        return {"ok": True}


def _tool_call(name, args, call_id):
    return {"name": name, "args": args, "id": call_id, "type": "tool_call"}


def _base_state(extra=None):
    from langchain_core.messages import AIMessageChunk

    state = {
        "messages": [AIMessageChunk(content="", tool_calls=[])],
        "run_id": "r1",
        "auth_header": "Bearer x",
        "tool_turns": 0,
        "allowed_tool_names": {"search_products"},
        "scoped_modules": ["product"],
        "expanded_modules": set(),
    }
    state.update(extra or {})
    return state


async def _call_tools(monkeypatch, tool_name, args, extra_state=None):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    state = _base_state(extra_state)
    state["messages"] = [type(state["messages"][0])(
        content="", tool_calls=[_tool_call(tool_name, args, "c1")])]
    return await manager_agent.call_tools_node(state)


async def test_goi_tool_bia_tra_goi_y_ten_gan_nhat(monkeypatch):
    result = await _call_tools(monkeypatch, "search_product", {})
    payload = json.loads(result["messages"][0].content)
    assert "không có tool" in payload["error"].lower()
    assert "search_products" in payload["error"]
    assert result["tool_not_found_counts"]["search_product"] == 1
    assert result["tools_disabled"] is False


async def test_goi_tool_bia_2_lan_chuyen_sang_khong_dung_tool(monkeypatch):
    result1 = await _call_tools(monkeypatch, "search_product", {})
    result2 = await _call_tools(
        monkeypatch, "search_product", {},
        {"tool_not_found_counts": result1["tool_not_found_counts"]},
    )
    payload = json.loads(result2["messages"][0].content)
    assert "dừng thử tool này" in payload["error"].lower()
    assert result2["tools_disabled"] is True


async def test_tools_disabled_khong_bind_tool_nao(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))

    class FakeLLM:
        def bind_tools(self, tools):
            self.bound = tools
            return self

        async def astream(self, messages):
            from langchain_core.messages import AIMessageChunk
            yield AIMessageChunk(content="Không tìm được dữ liệu phù hợp.")

    fake_llm = FakeLLM()
    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: fake_llm)
    from langchain_core.messages import HumanMessage
    state = _base_state({
        "messages": [HumanMessage(content="hoi")], "tools_disabled": True, "turns": 0,
        "permissions": [],
    })
    await manager_agent.call_model_node(state, {"configurable": {}})
    assert fake_llm.bound == []


def test_tool_da_go_co_the_thay_the_neu_da_khai_bao():
    specs = registry.load_tool_specs()
    specs["search_products_v0"] = registry.ToolSpec(
        name="search_products_v0", module="product", status="removed",
        replaced_by="search_products",
    )
    error = registry.resolve_tool_call_error(
        "search_products_v0", {"allowed_tool_names": {"search_products"}}, specs)
    assert error["kind"] == "tool_removed"
    assert "search_products" in error["message"]


def test_tool_da_go_khong_co_thay_the():
    specs = {"old_tool": registry.ToolSpec(name="old_tool", module="product", status="removed")}
    error = registry.resolve_tool_call_error("old_tool", {"allowed_tool_names": set()}, specs)
    assert error["kind"] == "tool_removed"
    assert "không có tool thay thế" in error["message"].lower()


async def test_goi_tool_thuoc_module_chua_nap_tu_dong_nap(monkeypatch):
    result = await _call_tools(
        monkeypatch, "get_order_status", {"order_id": 1},
        {"scoped_modules": ["product"], "allowed_tool_names": {"search_products"}},
    )
    payload = json.loads(result["messages"][0].content)
    assert payload.get("info") == "module_loaded"
    assert "sales" in result["expanded_modules"]
    assert result["module_expansions"] == 1


async def test_module_chi_tu_nap_toi_da_1_lan(monkeypatch):
    result = await _call_tools(
        monkeypatch, "get_order_status", {"order_id": 1},
        {
            "scoped_modules": ["product"],
            "allowed_tool_names": {"search_products"},
            "module_expansions": 1,
            "expanded_modules": {"inventory"},
        },
    )
    payload = json.loads(result["messages"][0].content)
    assert "error" in payload
    assert "không có quyền" in payload["error"].lower()


def test_registry_fingerprint_dung_trong_dispatch_khong_anh_huong_khi_sua_mo_ta():
    specs = registry.load_tool_specs()
    fp1 = registry.registry_fingerprint(specs)
    assert fp1 == registry.registry_fingerprint(registry.load_tool_specs())


class FakeManifestClient:
    def __init__(self, tools, build_id="v1"):
        self._tools = tools
        self._build_id = build_id

    async def get_tool_manifest(self):
        return {"tools": self._tools, "buildId": self._build_id}


async def test_verify_tool_contract_tu_vo_hieu_tool_thieu_endpoint(monkeypatch):
    registry._locally_disabled.clear()
    all_names = set(registry.load_tool_specs())
    missing_one = all_names - {"search_products"}
    result = await registry.verify_tool_contract(FakeManifestClient(list(missing_one)))
    assert result["missing_backend"] == ["search_products"]
    specs_after = registry.load_tool_specs()
    assert specs_after["search_products"].status == "removed"
    registry._locally_disabled.clear()


async def test_verify_tool_contract_khong_crash_khi_backend_loi():
    class BrokenClient:
        async def get_tool_manifest(self):
            raise RuntimeError("unreachable")

    result = await registry.verify_tool_contract(BrokenClient())
    assert result == {"missing_backend": [], "missing_spec": [], "stale_build": False}


async def test_verify_tool_contract_bao_stale_khi_lech_build_id(monkeypatch):
    monkeypatch.setattr(registry.get_settings(), "expected_build_id", "old-build", raising=False)
    all_names = set(registry.load_tool_specs())
    result = await registry.verify_tool_contract(FakeManifestClient(list(all_names), build_id="new-build"))
    assert result["stale_build"] is True


async def test_tool_flags_off_theo_snapshot_dau_run(monkeypatch):
    result = await _call_tools(
        monkeypatch, "search_products", {},
        {"tool_flags_snapshot": {"search_products": "off"}},
    )
    payload = json.loads(result["messages"][0].content)
    assert "hiện đang tắt" in payload["error"]


async def test_tool_off_giua_run_khong_anh_huong_snapshot_cu(monkeypatch):
    result = await _call_tools(
        monkeypatch, "search_products", {},
        {"tool_flags_snapshot": {}},
    )
    assert "hiện đang tắt" not in result["messages"][0].content


async def test_hua_tra_cuu_khong_goi_tool_khong_bi_bao_sai_la_thieu_quyen(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))

    from langchain_core.messages import AIMessageChunk, HumanMessage

    class StallingLLM:
        def bind_tools(self, tools):
            return self

        async def astream(self, messages):
            yield AIMessageChunk(content="Để tôi tìm giúp bạn danh sách sản phẩm này nhé.")

    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: StallingLLM())
    state = _base_state({
        "messages": [HumanMessage(content="Tôi chưa biết ID")],
        "permissions": ["Permissions.Order.ProductManagement.View"],
        "turns": 0,
    })
    result = await manager_agent.call_model_node(state, {"configurable": {}})
    final_text = result["messages"][0].content
    assert "không đủ quyền" not in final_text.lower()
    assert "tra cứu thêm" in final_text.lower()


async def test_get_product_stock_voi_id_bia_bi_chan(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    from langchain_core.messages import AIMessageChunk, HumanMessage

    state = _base_state({
        "messages": [
            HumanMessage(content="Tồn kho các sản phẩm nhông sên đĩa"),
            AIMessageChunk(content="", tool_calls=[_tool_call("get_product_stock", {"product_id": 12345}, "c1")]),
        ],
        "allowed_tool_names": {"search_products", "get_product_stock"},
        "scoped_modules": ["product", "inventory"],
        "known_ids": set(),
    })
    result = await manager_agent.call_tools_node(state)
    payload = json.loads(result["messages"][0].content)
    assert "search_products" in payload["error"]


async def test_get_product_stock_voi_id_tu_search_truoc_do_duoc_cho_qua(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    from langchain_core.messages import AIMessageChunk, HumanMessage

    state = _base_state({
        "messages": [
            HumanMessage(content="Tồn kho các sản phẩm nhông sên đĩa"),
            AIMessageChunk(content="", tool_calls=[_tool_call("get_product_stock", {"product_id": 5}, "c1")]),
        ],
        "allowed_tool_names": {"search_products", "get_product_stock"},
        "scoped_modules": ["product", "inventory"],
        "known_ids": {"5"},
    })
    result = await manager_agent.call_tools_node(state)
    assert "search_products" not in result["messages"][0].content


async def test_suy_nghi_bia_ten_san_pham_khong_lot_vao_text_delta(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    events = []
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: events.append)
    from langchain_core.messages import AIMessageChunk, HumanMessage

    class ThinkingThenToolLLM:
        def bind_tools(self, tools):
            return self

        async def astream(self, messages):
            chunks = [
                "<suy_nghi>Chắc chắn là xe Honda XYZ-999 huyền thoại, ",
                "để tôi tra giúp bạn.</suy_nghi>",
            ]
            for chunk in chunks:
                yield AIMessageChunk(content=chunk)
            yield AIMessageChunk(
                content="", tool_calls=[_tool_call("search_products", {"keyword": "x"}, "c1")])

    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: ThinkingThenToolLLM())
    state = _base_state({
        "messages": [HumanMessage(content="tìm sản phẩm x")],
        "permissions": ["Permissions.Order.ProductManagement.View"],
        "turns": 0,
    })
    result = await manager_agent.call_model_node(state, {"configurable": {}})

    text_deltas = "".join(payload for type_, payload in events if type_ == "text_delta")
    assert "Honda XYZ-999" not in text_deltas
    thinking_events = [payload for type_, payload in events if type_ == "thinking"]
    assert thinking_events and "Honda XYZ-999" in thinking_events[0]
    assert "Honda XYZ-999" not in result["messages"][0].content


async def test_suy_nghi_bi_ngat_giua_chung_khong_lo_ra_text_delta(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    events = []
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: events.append)
    from langchain_core.messages import AIMessageChunk, HumanMessage

    class CutOffThinkingLLM:
        def bind_tools(self, tools):
            return self

        async def astream(self, messages):
            yield AIMessageChunk(content="<suy_nghi>tên sản phẩm bịa Honda XYZ-999")

    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: CutOffThinkingLLM())
    state = _base_state({
        "messages": [HumanMessage(content="tìm sản phẩm x")],
        "permissions": ["Permissions.Order.ProductManagement.View"],
        "turns": 0,
    })
    await manager_agent.call_model_node(state, {"configurable": {}})

    text_deltas = "".join(payload for type_, payload in events if type_ == "text_delta")
    assert "Honda XYZ-999" not in text_deltas


async def test_text_kem_tool_call_bi_xoa_vi_chua_co_ket_qua_that(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    events = []
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: events.append)
    from langchain_core.messages import AIMessageChunk, HumanMessage

    class NarratingLLM:
        def bind_tools(self, tools):
            return self

        async def astream(self, messages):
            yield AIMessageChunk(
                content="Tôi đã tìm thấy sản phẩm X, để tôi kiểm tra tồn kho ngay.",
                tool_calls=[_tool_call("search_products", {"keyword": "x"}, "c1")],
            )

    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: NarratingLLM())
    state = _base_state({
        "messages": [HumanMessage(content="tìm sản phẩm x")],
        "permissions": ["Permissions.Order.ProductManagement.View"],
        "turns": 0,
    })
    result = await manager_agent.call_model_node(state, {"configurable": {}})

    assert result["messages"][0].content == ""
    assert result["messages"][0].tool_calls
    corrections = [p for t, p in events if t == "message_correction"]
    assert corrections == [""]


def test_sanitize_history_khong_giu_tool_call():
    history = [
        {"role": "user", "message": "tra cuu don hang 1"},
        {
            "role": "ai",
            "message": "Đơn hàng đang giao.",
            "tool_calls": [{"name": "get_order_status", "args": {"order_id": 1}}],
        },
    ]
    messages = build_history_messages({"history": history}, current_message="")
    for message in messages:
        assert not getattr(message, "tool_calls", None)
        assert not hasattr(message, "tool_calls") or message.tool_calls == []
