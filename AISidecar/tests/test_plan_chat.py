import sys

from app.schemas.plan_chat import PlanChatIntent, PlanChatOperation
from app.tools import registry


class FakeStructuredLlm:
    def __init__(self, result):
        self._result = result

    async def ainvoke(self, _prompt):
        return self._result


def _chat_module():
    return sys.modules["app.api.v1.chat"]


def test_interpret_plan_chat_edit_op_duoc_suy_ra_expected_tools(client, internal_secret, monkeypatch):
    chat_module = _chat_module()
    fake_result = PlanChatIntent(
        intent="edit_plan",
        operations=[PlanChatOperation(type="edit", step_id="s1", title="Lấy doanh thu", detail="Tra doanh thu tháng này")],
        reply="Đã sửa bước 1 theo yêu cầu của bạn.",
    )
    monkeypatch.setattr(chat_module, "_get_plan_chat_structured_llm", lambda: FakeStructuredLlm(fake_result))
    monkeypatch.setattr(chat_module, "load_tool_specs", lambda: {
        "get_sales_summary": registry.ToolSpec(name="get_sales_summary", module="sales", status="active"),
    })

    async def fake_infer_step_tools(step_text, allowed):
        return ["get_sales_summary"]

    monkeypatch.setattr(chat_module, "infer_step_tools", fake_infer_step_tools)

    resp = client.post(
        "/plan/interpret",
        json={"run_id": "r1", "message": "sửa bước 1 thành lấy doanh thu tháng này",
              "steps": [{"id": "s1", "order": 1, "title": "x", "detail": "y", "status": "pending"}]},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    body = resp.json()
    assert body["intent"] == "edit_plan"
    assert body["reply"] == "Đã sửa bước 1 theo yêu cầu của bạn."
    assert body["operations"][0]["expected_tools"] == ["get_sales_summary"]


def test_interpret_plan_chat_comment_op_khong_goi_infer_step_tools(client, internal_secret, monkeypatch):
    chat_module = _chat_module()
    fake_result = PlanChatIntent(
        intent="edit_plan",
        operations=[PlanChatOperation(type="comment", step_id="s1", comment="Chưa đủ chi tiết")],
        reply="Đã ghi nhận bình luận của bạn.",
    )
    monkeypatch.setattr(chat_module, "_get_plan_chat_structured_llm", lambda: FakeStructuredLlm(fake_result))

    async def fail_if_called(*_a, **_k):
        raise AssertionError("Không được gọi infer_step_tools cho operation comment")

    monkeypatch.setattr(chat_module, "infer_step_tools", fail_if_called)

    resp = client.post(
        "/plan/interpret",
        json={"run_id": "r1", "message": "bước này chưa rõ", "steps": [], "target_step_id": "s1"},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    body = resp.json()
    assert body["operations"][0]["type"] == "comment"
    assert "expected_tools" not in body["operations"][0]


def test_interpret_plan_chat_loi_llm_thi_tra_ve_unclear(client, internal_secret, monkeypatch):
    chat_module = _chat_module()

    def raise_error():
        raise RuntimeError("LLM down")

    monkeypatch.setattr(chat_module, "_get_plan_chat_structured_llm", raise_error)

    resp = client.post(
        "/plan/interpret",
        json={"run_id": "r1", "message": "sửa giúp tôi", "steps": []},
        headers={"X-Internal-Secret": internal_secret},
    )

    assert resp.status_code == 200
    body = resp.json()
    assert body["intent"] == "unclear"
    assert body["operations"] == []
