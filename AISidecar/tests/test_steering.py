import asyncio
import uuid

from langchain_core.language_models.fake import FakeListLLM
from langchain_core.messages import HumanMessage, SystemMessage

from app.agents import manager_agent


async def test_absorb_steering_rong_khong_tao_message_thua(monkeypatch):
    class EmptyBackendClient:
        def __init__(self, auth_header):
            pass

        async def pull_pending_steering(self, run_id):
            return []

    monkeypatch.setattr(manager_agent, "BackendClient", EmptyBackendClient)

    result = await manager_agent.absorb_steering_node({"carried_steering": [], "auth_header": "Bearer x", "run_id": "r1"})
    assert result == {"absorbed_count": 0, "carried_steering": []}


def test_build_steering_message_interrupt_noi_ro_dinh_chinh():
    msg = manager_agent.build_steering_message({"content": "tháng trước", "mode": "interrupt"})
    assert "ĐÍNH CHÍNH" in msg.content
    assert "tháng trước" in msg.content


def test_build_steering_message_queue_noi_ro_bo_sung():
    msg = manager_agent.build_steering_message({"content": "thêm số đơn hàng", "mode": "queue"})
    assert "BỔ SUNG" in msg.content
    assert "thêm số đơn hàng" in msg.content


def test_route_after_absorb_luot_dau_luon_tiep_tuc():
    assert manager_agent.route_after_absorb({"turns": 0}) == "continue"


def test_route_after_absorb_ket_thuc_khi_khong_con_gi_cho():
    assert manager_agent.route_after_absorb({"turns": 1, "absorbed_count": 0}) == "end"


def test_route_after_absorb_tiep_tuc_khi_con_steering_cho():
    assert manager_agent.route_after_absorb({"turns": 1, "absorbed_count": 2}) == "continue"


def test_route_after_absorb_ket_thuc_ngay_khi_bi_huy():
    assert manager_agent.route_after_absorb({"turns": 1, "absorbed_count": 3, "cancelled": True}) == "end"


class FakeBackendClient:
    calls = []

    def __init__(self, auth_header):
        self.auth_header = auth_header

    async def pull_pending_steering(self, run_id):
        FakeBackendClient.calls.append(run_id)
        if len(FakeBackendClient.calls) == 2:
            return [{"content": "thêm số đơn hàng nữa", "mode": "queue"}]
        return []


async def test_graph_phat_turn_boundary_khi_co_steering_queue(monkeypatch):
    FakeBackendClient.calls = []
    fake_llm = FakeListLLM(responses=["trả lời 1", "trả lời 2"])
    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: fake_llm)
    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)

    graph = manager_agent.build_graph()
    run_id = str(uuid.uuid4())
    config = {"configurable": {"thread_id": run_id, "cancel_event": None}}
    state = {
        "messages": [SystemMessage(content="hệ thống"), HumanMessage(content="doanh thu tháng này")],
        "run_id": run_id,
        "auth_header": "Bearer x",
        "turns": 0,
        "absorbed_count": 0,
        "carried_steering": [],
        "cancelled": False,
    }

    chunks = [c async for c in graph.astream(state, config=config, stream_mode="custom")]

    types = [type_ for type_, _ in chunks]
    first_delta_idx = types.index("text_delta")
    boundary_idx = types.index("turn_boundary")
    second_delta_idx = types.index("text_delta", boundary_idx + 1)
    assert first_delta_idx < boundary_idx < second_delta_idx

    text_deltas = "".join(payload for type_, payload in chunks if type_ == "text_delta")
    assert text_deltas == "trả lời 1trả lời 2"

    final_messages = graph.get_state(config).values["messages"]
    human_contents = [m.content for m in final_messages if isinstance(m, HumanMessage)]
    assert any("BỔ SUNG" in content for content in human_contents)


async def test_graph_dung_ngay_khi_bi_huy(monkeypatch):
    fake_llm = FakeListLLM(responses=["trả lời sẽ bị huỷ", "không nên tới đây"])
    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: fake_llm)

    class NoSteeringBackendClient:
        def __init__(self, auth_header):
            pass

        async def pull_pending_steering(self, run_id):
            return []

    monkeypatch.setattr(manager_agent, "BackendClient", NoSteeringBackendClient)

    cancel_event = asyncio.Event()
    cancel_event.set()

    graph = manager_agent.build_graph()
    run_id = str(uuid.uuid4())
    config = {"configurable": {"thread_id": run_id, "cancel_event": cancel_event}}
    state = {
        "messages": [HumanMessage(content="huỷ ngay")],
        "run_id": run_id,
        "auth_header": "Bearer x",
        "turns": 0,
        "absorbed_count": 0,
        "carried_steering": [],
        "cancelled": False,
    }

    chunks = [c async for c in graph.astream(state, config=config, stream_mode="custom")]

    assert not any(type_ == "text_delta" for type_, _ in chunks)
    assert graph.get_state(config).values["cancelled"] is True
