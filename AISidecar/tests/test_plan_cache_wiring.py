from langchain_core.language_models.fake import FakeListLLM
from langchain_core.messages import HumanMessage

from app.agents import manager_agent
from app.services import plan_cache
from app.tools import registry


def _active_specs():
    return {"get_sales_summary": registry.ToolSpec(name="get_sales_summary", module="sales", status="active")}


async def test_plan_node_cache_hit_bo_qua_llm_sinh_plan(monkeypatch):
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    monkeypatch.setattr(manager_agent, "load_tool_specs", _active_specs)

    def fake_get_llm(**kwargs):
        raise AssertionError("Không được gọi LLM sinh plan mới khi cache hit")

    monkeypatch.setattr(manager_agent, "get_llm", fake_get_llm)

    async def fake_fill_slots(slots, question, server_date):
        assert server_date == "2026-08-04"
        return {"from_date": "2026-07-01", "to_date": "2026-07-31"}

    monkeypatch.setattr(plan_cache, "fill_slots", fake_fill_slots)

    steps_template = [{
        "id": "s1", "order": 1, "title": "Doanh thu {{from_date}} - {{to_date}}",
        "detail": "Gọi get_sales_summary", "expectedTools": ["get_sales_summary"],
    }]
    slots = [{"name": "from_date", "type": "date", "description": "Ngày bắt đầu"},
             {"name": "to_date", "type": "date", "description": "Ngày kết thúc"}]
    added_steps = []
    events = []

    class FakeBackendClient:
        def __init__(self, auth_header):
            pass

        async def start_plan(self, run_id, fingerprint):
            return {"planId": "p1"}

        async def get_plan(self, run_id):
            return {"planId": "p1", "version": 1, "status": "Drafting", "steps": []}

        async def find_plan_template(self, intent_hash, module):
            assert module == "sales"
            return {
                "templateId": "t1", "module": module, "status": "active",
                "toolRegistryFingerprint": "outdated-fingerprint",
                "requiredTools": ["get_sales_summary"], "requiredPermissions": [],
                "stepsTemplate": steps_template, "slots": slots,
            }

        async def add_plan_step(self, run_id, title, detail, expected_tools):
            added_steps.append({"title": title, "detail": detail, "expectedTools": expected_tools})
            return {"id": f"s{len(added_steps)}", "order": len(added_steps), "title": title,
                    "detail": detail, "expectedTools": expected_tools, "status": "pending",
                    "editedByUser": False, "result": None}

        async def mark_plan_ready(self, run_id):
            pass

    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: events.append)

    result = await manager_agent.plan_node({
        "messages": [HumanMessage(content="doanh thu tháng 7")],
        "run_id": "r1",
        "auth_header": "Bearer x",
        "scoped_modules": ["sales"],
        "permissions": [],
        "server_date": "2026-08-04",
    })

    assert result == {"plan_id": "p1"}
    assert added_steps == [{
        "title": "Doanh thu 2026-07-01 - 2026-07-31",
        "detail": "Gọi get_sales_summary",
        "expectedTools": ["get_sales_summary"],
    }]
    assert any(t == "plan_cache_hit" for t, _ in events)


async def test_plan_node_cache_miss_van_sinh_plan_bang_llm(monkeypatch):
    monkeypatch.setattr(manager_agent, "get_stream_writer", lambda: (lambda *_: None))
    monkeypatch.setattr(manager_agent, "build_plan_prompt", lambda *a, **k: "PROMPT")
    monkeypatch.setattr(manager_agent, "load_tool_specs", _active_specs)
    monkeypatch.setattr(manager_agent, "rag_enabled", lambda: False)

    fake_llm = FakeListLLM(responses=[
        "### BƯỚC 1: Lấy doanh thu\nGọi get_sales_summary\nTOOLS: get_sales_summary",
    ])
    monkeypatch.setattr(manager_agent, "get_llm", lambda **kwargs: fake_llm)

    added_steps = []

    class FakeBackendClient:
        def __init__(self, auth_header):
            pass

        async def start_plan(self, run_id, fingerprint):
            return {"planId": "p1"}

        async def get_plan(self, run_id):
            return {"planId": "p1", "version": 1, "status": "Drafting", "steps": []}

        async def find_plan_template(self, intent_hash, module):
            return None

        async def add_plan_step(self, run_id, title, detail, expected_tools):
            added_steps.append(title)
            return {"id": "s1", "order": 1, "title": title, "detail": detail,
                    "expectedTools": expected_tools, "status": "pending",
                    "editedByUser": False, "result": None}

        async def mark_plan_ready(self, run_id):
            pass

    monkeypatch.setattr(manager_agent, "BackendClient", FakeBackendClient)

    result = await manager_agent.plan_node({
        "messages": [HumanMessage(content="doanh thu tháng 7")],
        "run_id": "r1",
        "auth_header": "Bearer x",
        "scoped_modules": ["sales"],
        "permissions": [],
    })

    assert result == {"plan_id": "p1"}
    assert added_steps == ["Lấy doanh thu"]


def test_validate_plan_template_chan_khi_thieu_quyen():
    tpl = {"status": "active", "toolRegistryFingerprint": "x", "requiredTools": [],
           "requiredPermissions": ["Permissions.Admin.DashboardManagement.View"]}
    assert manager_agent.validate_plan_template(tpl, {"permissions": []}) is False


def test_validate_plan_template_cho_qua_khi_du_quyen():
    tpl = {"status": "active", "toolRegistryFingerprint": "x", "requiredTools": [],
           "requiredPermissions": ["Permissions.Admin.DashboardManagement.View"]}
    assert manager_agent.validate_plan_template(
        tpl, {"permissions": ["Permissions.Admin.DashboardManagement.View"]}) is True


def test_validate_plan_template_chan_khi_status_khong_active():
    tpl = {"status": "stale", "toolRegistryFingerprint": "x", "requiredTools": [], "requiredPermissions": []}
    assert manager_agent.validate_plan_template(tpl, {}) is False


def test_validate_plan_template_chan_khi_tool_bi_go(monkeypatch):
    monkeypatch.setattr(manager_agent, "load_tool_specs", lambda: {})
    tpl = {"status": "active", "toolRegistryFingerprint": "khac-fingerprint",
           "requiredTools": ["tool_da_bi_go"], "requiredPermissions": []}
    assert manager_agent.validate_plan_template(tpl, {"permissions": []}) is False
