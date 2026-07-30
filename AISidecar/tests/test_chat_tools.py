import json
import uuid

from langchain_core.messages import AIMessageChunk, HumanMessage, SystemMessage

from app.agents import manager_agent
from app.services.chat_tools import build_tools, describe_args


def _fake_envelope(tool_path, payload):
    return {
        "items": [{"tool_path": tool_path, "payload": payload}],
        "totalCount": 1,
        "truncated": False,
        "asOf": "2026-07-26T09:15:00+07:00",
        "timezone": "Asia/Ho_Chi_Minh",
        "source": "fake",
        "filtersApplied": {},
        "definition": None,
        "currency": None,
        "warnings": [],
    }


class FakeBackendClient:
    def __init__(self, auth_header):
        self.auth_header = auth_header

    async def pull_pending_steering(self, run_id):
        return []

    async def call_tool(self, tool_path, payload):
        return _fake_envelope(tool_path, payload)


class FailingBackendClient(FakeBackendClient):
    async def call_tool(self, tool_path, payload):
        raise RuntimeError("backend unreachable")


def _tool_call(name, args, call_id):
    return {"name": name, "args": args, "id": call_id, "type": "tool_call"}


class OneShotToolLLM:
    def __init__(self, tool_name="get_order_status", args=None):
        self.tool_name = tool_name
        self.args = args or {"order_id": 1}
        self.calls = 0

    async def astream(self, messages):
        self.calls += 1
        yield AIMessageChunk(
            content="",
            tool_calls=[_tool_call(self.tool_name, self.args, f"call-{self.calls}")],
        )


def _base_state(run_id, extra=None):
    state = {
        "messages": [SystemMessage(content="he thong"), HumanMessage(content="hoi don hang")],
        "run_id": run_id,
        "auth_header": "Bearer x",
        "turns": 0,
        "absorbed_count": 0,
        "carried_steering": [],
        "cancelled": False,
        "tool_turns": 0,
        "tool_limit_reached": False,
    }
    state.update(extra or {})
    return state


def test_build_tools_tra_dung_ten_va_mo_ta():
    tools = build_tools(FakeBackendClient("Bearer x"))
    names = {t.name for t in tools}
    assert names == {
        "search_products", "get_product_stock", "get_low_stock_products",
        "get_order_status", "get_sales_summary", "get_top_selling",
    }
    for tool in tools:
        assert tool.description


def test_build_tools_loc_theo_allowed_names():
    tools = build_tools(FakeBackendClient("Bearer x"), {"search_products"})
    assert {t.name for t in tools} == {"search_products"}


def test_describe_args_dung_label_va_bo_qua_gia_tri_rong():
    summary = describe_args("search_products", {"keyword": "nhông sên đĩa DID", "limit": ""})
    assert summary == "Từ khóa: nhông sên đĩa DID"


def test_describe_args_nhieu_field_noi_bang_phay():
    summary = describe_args("get_top_selling", {"from_date": "2026-07-01", "to_date": "", "limit": 5})
    assert summary == "Top N: 5"


def test_describe_args_bo_qua_from_to_date_vi_da_hien_o_filtersApplied():
    summary = describe_args(
        "get_sales_summary", {"from_date": "2026-06-01", "to_date": "2026-06-30", "limit": 10})
    assert "2026-06-01" not in summary
    assert "2026-06-30" not in summary


async def test_call_tools_node_goi_dung_tool_va_emit_start_end(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    events = []
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: events.append)

    state = _base_state("r1", {
        "messages": [AIMessageChunk(
            content="",
            tool_calls=[_tool_call("get_order_status", {"order_id": 1}, "call-1")],
        )],
    })
    result = await manager_agent.call_tools_node(state)

    assert result["tool_turns"] == 1
    assert result["tool_limit_reached"] is False
    tool_message = result["messages"][0]
    payload = json.loads(tool_message.content)
    assert payload["items"][0]["tool_path"] == "orders/status"
    assert payload["items"][0]["payload"] == {"order_id": 1}
    assert tool_message.tool_call_id == "call-1"
    tool_start_type, tool_start_payload = events[0]
    assert tool_start_type == "tool_start"
    assert json.loads(tool_start_payload) == {"name": "get_order_status", "summary": "Mã đơn hàng: 1"}
    tool_end_type, tool_end_payload = events[1]
    assert tool_end_type == "tool_end"
    assert json.loads(tool_end_payload) == {
        "name": "get_order_status", "truncated": False, "totalCount": 1,
        "asOf": "2026-07-26T09:15:00+07:00", "warnings": [], "filtersApplied": {},
    }


async def test_call_tools_node_tool_loi_khong_crash(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FailingBackendClient)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))

    state = _base_state("r2", {
        "messages": [AIMessageChunk(
            content="",
            tool_calls=[_tool_call("get_order_status", {"order_id": 2}, "call-2")],
        )],
    })
    result = await manager_agent.call_tools_node(state)

    tool_message = result["messages"][0]
    payload = json.loads(tool_message.content)
    assert "error" in payload
    assert tool_message.tool_call_id == "call-2"


async def test_cap_vong_lap_tool_dung_dung_nguong(monkeypatch):
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))

    fake_llm = OneShotToolLLM()
    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: fake_llm)

    graph = manager_agent.build_graph()
    run_id = str(uuid.uuid4())
    config = {"configurable": {"thread_id": run_id, "cancel_event": None}}
    state = _base_state(run_id)

    [c async for c in graph.astream(state, config=config, stream_mode="custom")]

    final = graph.get_state(config).values
    assert final["tool_turns"] == manager_agent.MAX_TOOL_TURNS + 1
    assert final["tool_limit_reached"] is True
    assert fake_llm.calls == manager_agent.MAX_TOOL_TURNS + 1
