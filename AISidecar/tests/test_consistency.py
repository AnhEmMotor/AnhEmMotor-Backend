from app.agents import manager_agent
from app.core.redaction import make_tool_preview
from app.services.run_snapshot import RunSnapshot


async def test_run_snapshot_get_cung_tool_va_args_chi_goi_fetcher_1_lan():
    calls = []

    async def fetcher():
        calls.append(1)
        return {"asOf": "2026-07-28T09:15:02+07:00", "value": 12}

    snapshot = RunSnapshot("run-1")
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher)
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher)

    assert len(calls) == 1


async def test_run_snapshot_as_of_neo_theo_lan_doc_dau_tien():
    async def fetcher_1():
        return {"asOf": "2026-07-28T09:15:02+07:00", "value": 12}

    async def fetcher_2():
        return {"asOf": "2026-07-28T09:20:00+07:00", "value": 98}

    snapshot = RunSnapshot("run-1")
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher_1)
    await snapshot.get("get_price", {"model": "SH150i"}, fetcher_2)

    assert snapshot.as_of == "2026-07-28T09:15:02+07:00"


async def test_run_snapshot_lech_asof_qua_60s_sinh_warning():
    async def fetcher_1():
        return {"asOf": "2026-07-28T09:15:00+07:00", "value": 12}

    async def fetcher_2():
        return {"asOf": "2026-07-28T09:16:30+07:00", "value": 98}

    snapshot = RunSnapshot("run-1")
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher_1)
    await snapshot.get("get_price", {"model": "SH150i"}, fetcher_2)

    assert snapshot.warnings() == ["Dữ liệu được lấy ở các thời điểm cách nhau hơn 1 phút"]


async def test_run_snapshot_khong_lech_thi_khong_co_warning():
    async def fetcher_1():
        return {"asOf": "2026-07-28T09:15:00+07:00", "value": 12}

    async def fetcher_2():
        return {"asOf": "2026-07-28T09:15:30+07:00", "value": 98}

    snapshot = RunSnapshot("run-1")
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher_1)
    await snapshot.get("get_price", {"model": "SH150i"}, fetcher_2)

    assert snapshot.warnings() == []


class _FakeBackendClientWithSensitiveResult:
    def __init__(self, auth_header):
        pass

    async def pull_pending_steering(self, run_id):
        return []

    async def call_tool(self, tool_path, payload):
        return _sensitive_order_result()


def _sensitive_order_result() -> dict:
    return {
        "items": [{"orderId": 1, "customerName": "Nguyễn Văn A", "statusId": "shipped"}],
        "totalCount": 1,
        "truncated": False,
        "asOf": "2026-08-04T09:00:00+07:00",
        "timezone": "Asia/Ho_Chi_Minh",
        "source": "sql",
        "filtersApplied": {},
    }


async def test_du_lieu_vao_llm_khong_bi_che_con_fe_thi_co(monkeypatch):
    from langchain_core.messages import AIMessageChunk

    monkeypatch.setattr(manager_agent, "BackendClient", _FakeBackendClientWithSensitiveResult)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))

    state = {
        "messages": [AIMessageChunk(content="", tool_calls=[
            {"name": "get_order_status", "args": {"keyword": "Nguyễn Văn A"}, "id": "c1", "type": "tool_call"},
        ])],
        "run_id": "r1",
        "auth_header": "Bearer x",
        "tool_turns": 0,
        "allowed_tool_names": {"get_order_status"},
        "scoped_modules": ["sales"],
        "expanded_modules": set(),
    }

    result = await manager_agent.call_tools_node(state)

    tool_message = result["messages"][0]
    assert "Nguyễn Văn A" in tool_message.content
    assert "***" not in tool_message.content

    fe_preview = make_tool_preview(_sensitive_order_result())
    assert "Nguyễn Văn A" not in fe_preview["preview"]
    assert "***" in fe_preview["preview"]
