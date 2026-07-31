import sys
import uuid

from langchain_core.language_models.fake import FakeListLLM
from langchain_core.messages import HumanMessage

from app.agents import manager_agent
from app.tools import registry


def test_classify_node_nhan_dien_tu_khoa_da_buoc():
    result = manager_agent.classify_node({"messages": [HumanMessage(content="Chuẩn bị báo cáo tồn kho quý này")]})
    assert result == {"needs_plan": True}


def test_classify_node_bo_qua_cau_hoi_don_gian():
    result = manager_agent.classify_node({"messages": [HumanMessage(content="Xe SH giá bao nhiêu?")]})
    assert result == {"needs_plan": False}


def test_route_after_classify_di_dung_nhanh():
    assert manager_agent.route_after_classify({"needs_plan": True}) == "plan"
    assert manager_agent.route_after_classify({"needs_plan": False}) == "absorb_steering"
    assert manager_agent.route_after_classify({"needs_plan": False, "plan_id": "p1"}) == "execute_step"
    assert manager_agent.route_after_classify({"needs_plan": True, "plan_id": "p1"}) == "execute_step"


def test_split_plan_blocks_khop_khoi_da_dong_bien_boi_khoi_ke_tiep():
    text = (
        "### BƯỚC 1: Lấy tồn kho thấp\n"
        "Gọi tool lấy danh sách sản phẩm tồn kho thấp\n"
        "TOOLS: get_low_stock_products\n"
        "### BƯỚC 2: Tính giá trị tồn kho\n"
        "Tính tổng giá trị theo danh mục\n"
        "TOOLS: get_inventory_value, get_categories\n"
    )
    blocks = manager_agent._split_plan_blocks(text)
    assert blocks == [{
        "title": "Lấy tồn kho thấp",
        "detail": "Gọi tool lấy danh sách sản phẩm tồn kho thấp",
        "tools": ["get_low_stock_products"],
    }]


def test_split_plan_blocks_khoi_cuoi_can_sentinel_moi_khop():
    text = "### BƯỚC 1: Lấy tồn kho thấp\nGọi tool lấy danh sách\nTOOLS: get_low_stock_products\n"
    assert manager_agent._split_plan_blocks(text) == []

    with_sentinel = text + manager_agent._PLAN_BLOCK_SENTINEL
    blocks = manager_agent._split_plan_blocks(with_sentinel)
    assert blocks == [{
        "title": "Lấy tồn kho thấp",
        "detail": "Gọi tool lấy danh sách",
        "tools": ["get_low_stock_products"],
    }]


def test_split_plan_blocks_khong_khop_khoi_thieu_dong_tools():
    text = "### BƯỚC 1: Lấy tồn kho thấp\nGọi tool lấy danh sách\n" + manager_agent._PLAN_BLOCK_SENTINEL
    assert manager_agent._split_plan_blocks(text) == []


async def test_plan_node_dua_locked_steps_vao_prompt(monkeypatch):
    captured = {}

    def fake_build_plan_prompt(user_request, existing_steps):
        captured["request"] = user_request
        captured["existing_steps"] = existing_steps
        return "PROMPT"

    monkeypatch.setattr(manager_agent, "build_plan_prompt", fake_build_plan_prompt)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    fake_llm = FakeListLLM(responses=[
        "### BƯỚC 2: Bước mới\nMô tả bước mới\nTOOLS: get_orders",
    ])
    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: fake_llm)

    locked_step = {
        "id": "s1", "order": 1, "title": "Bước đã sửa", "detail": "chi tiết đã sửa",
        "expectedTools": [], "status": "pending", "editedByUser": True, "result": None,
    }

    class FakeBackendClient:
        def __init__(self, auth_header):
            pass

        async def start_plan(self, run_id, fingerprint):
            return {"planId": "p1"}

        async def get_plan(self, run_id):
            return {"planId": "p1", "version": 1, "status": "Drafting", "steps": [locked_step]}

        async def add_plan_step(self, run_id, title, detail, expected_tools):
            return {"id": "s2", "order": 2, "title": title, "detail": detail,
                    "expectedTools": expected_tools, "status": "pending", "editedByUser": False, "result": None}

        async def mark_plan_ready(self, run_id):
            pass

    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)

    result = await manager_agent.plan_node({
        "messages": [HumanMessage(content="Chuẩn bị báo cáo")],
        "run_id": "r1",
        "auth_header": "Bearer x",
    })

    assert result == {"plan_id": "p1"}
    assert captured["existing_steps"] == [locked_step]
    assert captured["request"] == "Chuẩn bị báo cáo"


async def test_execute_step_node_lay_dung_buoc_pending_ke_tiep(monkeypatch):
    events = []
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda evt: events.append(evt)))
    status_calls = []

    class FakeBackendClient:
        def __init__(self, auth_header):
            pass

        async def get_plan(self, run_id):
            return {"steps": [
                {"id": "s1", "order": 1, "title": "Đã xong", "detail": "d", "expectedTools": [],
                 "status": "done", "editedByUser": False, "result": "ok"},
                {"id": "s2", "order": 2, "title": "Chưa làm", "detail": "d2", "expectedTools": ["get_orders"],
                 "status": "pending", "editedByUser": False, "result": None},
            ]}

        async def update_plan_step_status(self, run_id, step_id, status, result=None):
            status_calls.append((step_id, status, result))

    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)

    result = await manager_agent.execute_step_node({"run_id": "r1", "auth_header": "Bearer x"})

    assert result["current_plan_step"]["id"] == "s2"
    assert result["plan_finished"] is False
    assert status_calls == [("s2", "running", None)]
    assert ("plan_step_started", '{"stepId": "s2"}') in events


async def test_execute_step_node_het_buoc_thi_plan_finished(monkeypatch):
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))

    class FakeBackendClient:
        def __init__(self, auth_header):
            pass

        async def get_plan(self, run_id):
            return {"steps": [
                {"id": "s1", "order": 1, "title": "Đã xong", "detail": "d", "expectedTools": [],
                 "status": "done", "editedByUser": False, "result": "ok"},
            ]}

    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)

    result = await manager_agent.execute_step_node({"run_id": "r1", "auth_header": "Bearer x"})
    assert result == {"current_plan_step": None, "plan_finished": True, "plan_approved": True}


async def test_step_completed_node_ghi_ket_qua_va_phat_event(monkeypatch):
    events = []
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda evt: events.append(evt)))
    status_calls = []

    class FakeBackendClient:
        def __init__(self, auth_header):
            pass

        async def update_plan_step_status(self, run_id, step_id, status, result=None):
            status_calls.append((step_id, status, result))

    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)

    from langchain_core.messages import AIMessage
    state = {
        "run_id": "r1", "auth_header": "Bearer x",
        "current_plan_step": {"id": "s2", "order": 2, "title": "x", "detail": "y",
                               "expectedTools": [], "status": "running", "editedByUser": False, "result": None},
        "messages": [AIMessage(content="Đã tra xong: còn 3 sản phẩm.")],
    }
    result = await manager_agent.step_completed_node(state)

    assert result == {"current_plan_step": None}
    assert status_calls == [("s2", "done", "Đã tra xong: còn 3 sản phẩm.")]
    assert any(t == "plan_step_completed" for t, _ in events)


def test_route_after_model_uu_tien_carried_steering_hon_step_completed():
    from langchain_core.messages import AIMessage
    state = {
        "messages": [AIMessage(content="trả lời")],
        "carried_steering": [{"content": "à nhầm", "mode": "interrupt"}],
        "current_plan_step": {"id": "s1"},
    }
    assert manager_agent.route_after_model(state) == "absorb_steering"


def test_route_after_model_step_completed_khi_dang_thuc_thi_plan():
    from langchain_core.messages import AIMessage
    state = {
        "messages": [AIMessage(content="trả lời")],
        "carried_steering": [],
        "current_plan_step": {"id": "s1"},
    }
    assert manager_agent.route_after_model(state) == "step_completed"


def test_route_after_model_khong_plan_thi_ve_absorb_steering():
    from langchain_core.messages import AIMessage
    state = {"messages": [AIMessage(content="trả lời")], "carried_steering": []}
    assert manager_agent.route_after_model(state) == "absorb_steering"


def test_route_after_tools_gioi_han_thi_step_completed_khi_dang_plan():
    assert manager_agent.route_after_tools(
        {"tool_limit_reached": True, "current_plan_step": {"id": "s1"}}) == "step_completed"


def test_route_after_tools_gioi_han_thi_absorb_steering_khi_khong_plan():
    assert manager_agent.route_after_tools({"tool_limit_reached": True, "current_plan_step": None}) == "absorb_steering"


def test_route_after_execute_step():
    assert manager_agent.route_after_execute_step({"plan_finished": True}) == "summarize"
    assert manager_agent.route_after_execute_step({"plan_finished": False}) == "call_model"


async def test_graph_ket_thuc_sau_plan_ready_lan_goi_sau_seed_plan_id_chay_toi_summarize(monkeypatch):
    fake_llm = FakeListLLM(responses=[
        "### BƯỚC 1: Lấy tồn kho thấp\nGọi tool lấy tồn kho thấp\nTOOLS: get_low_stock_products",
        "Tồn kho thấp hiện có 3 sản phẩm.",
    ])
    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: fake_llm)

    plan_store = {"steps": [], "status": "Drafting"}

    class FakePlanBackendClient:
        def __init__(self, auth_header):
            pass

        async def start_plan(self, run_id, fingerprint):
            return {"planId": "p1"}

        async def get_plan(self, run_id):
            return {"planId": "p1", "version": 1, "status": plan_store["status"], "steps": plan_store["steps"]}

        async def add_plan_step(self, run_id, title, detail, expected_tools):
            step = {
                "id": f"s{len(plan_store['steps']) + 1}", "order": len(plan_store["steps"]) + 1,
                "title": title, "detail": detail, "expectedTools": expected_tools,
                "status": "done", "editedByUser": False, "result": "3 sản phẩm tồn kho thấp",
            }
            plan_store["steps"].append(step)
            return step

        async def mark_plan_ready(self, run_id):
            plan_store["status"] = "Ready"

        async def pull_pending_steering(self, run_id):
            return []

    monkeypatch.setattr(manager_agent, "BackendClient", FakePlanBackendClient)

    graph = manager_agent.build_graph()
    run_id = str(uuid.uuid4())
    config = {"configurable": {"thread_id": run_id, "cancel_event": None}}
    draft_state = {
        "messages": [HumanMessage(content="Chuẩn bị báo cáo tồn kho quý này")],
        "run_id": run_id,
        "auth_header": "Bearer x",
        "turns": 0,
        "absorbed_count": 0,
        "carried_steering": [],
        "cancelled": False,
        "tool_turns": 0,
    }

    chunks = [c async for c in graph.astream(draft_state, config=config, stream_mode="custom")]
    types = [t for t, _ in chunks]

    assert "plan_started" in types
    assert "plan_step_added" in types
    assert "plan_ready" in types
    snapshot = await graph.aget_state(config)
    assert snapshot.next == ()

    plan_store["status"] = "Executing"

    execute_state = {**draft_state, "plan_id": "p1"}
    chunks2 = [c async for c in graph.astream(execute_state, config=config, stream_mode="custom")]
    text_deltas = "".join(payload for type_, payload in chunks2 if type_ == "text_delta")
    assert text_deltas == "Tồn kho thấp hiện có 3 sản phẩm."

    final = (await graph.aget_state(config)).values
    assert final["messages"][-1].content == "Tồn kho thấp hiện có 3 sản phẩm."
    assert final["plan_approved"] is True


def test_revalidate_plan_fingerprint_khop_thi_ok(client, internal_secret, monkeypatch):
    chat_module = sys.modules["app.api.v1.chat"]
    monkeypatch.setattr(chat_module, "registry_fingerprint", lambda: "abc123")
    resp = client.post(
        "/plan/revalidate",
        json={"run_id": "r1", "expected_tools": ["get_stock"], "fingerprint": "abc123"},
        headers={"X-Internal-Secret": internal_secret},
    )
    assert resp.status_code == 200
    assert resp.json() == {"ok": True, "unavailable_tools": []}


def test_revalidate_plan_khong_co_fingerprint_thi_ok(client, internal_secret):
    resp = client.post(
        "/plan/revalidate",
        json={"run_id": "r1", "expected_tools": ["get_stock"], "fingerprint": ""},
        headers={"X-Internal-Secret": internal_secret},
    )
    assert resp.status_code == 200
    assert resp.json() == {"ok": True, "unavailable_tools": []}


def test_revalidate_plan_tool_bi_go_thi_bao_khong_kha_dung(client, internal_secret, monkeypatch):
    chat_module = sys.modules["app.api.v1.chat"]
    monkeypatch.setattr(chat_module, "registry_fingerprint", lambda: "new-fp")
    monkeypatch.setattr(chat_module, "load_tool_specs", lambda: {
        "get_orders": registry.ToolSpec(name="get_orders", module="order", status="active"),
    })
    resp = client.post(
        "/plan/revalidate",
        json={"run_id": "r1", "expected_tools": ["get_stock", "get_orders"], "fingerprint": "old-fp"},
        headers={"X-Internal-Secret": internal_secret},
    )
    assert resp.status_code == 200
    body = resp.json()
    assert body["ok"] is False
    assert body["unavailable_tools"] == ["get_stock"]
