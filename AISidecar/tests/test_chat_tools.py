import json
import uuid

from langchain_core.messages import AIMessageChunk, HumanMessage, SystemMessage

from app.agents import manager_agent
from app.services.chat_tools import SUMMARIZERS, build_tools, describe_args, load_catalog, summarize_result


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
        "list_booking_appointments", "get_product_detail", "get_inventory_report",
        "list_orders", "get_order_statistics", "get_customer_profile", "search_customers",
        "list_repair_orders", "get_repair_order_detail", "list_warranty_claims",
        "get_pnl_report", "get_suppliers_with_debt", "get_shipment_tracking",
        "get_dashboard_overview",
        "get_product_price_list", "list_brands", "list_categories",
        "get_supplier_prices_for_variant", "search_suppliers", "get_supplier_statistics",
        "get_inventory_ledger", "list_inventory_receipts", "get_inventory_receipt_detail",
        "list_purchase_requests", "get_purchase_request_detail",
        "get_revenue_by_category", "get_sales_report", "get_recent_transactions",
        "list_sales_contracts", "list_finance_contracts", "list_supplier_contracts", "list_vouchers",
        "get_lead_pipeline", "get_lead_detail", "list_contacts", "get_loyalty_members",
        "get_warranty_claim_detail", "get_warranty_terms", "list_workshop_payments",
        "list_bookings", "list_services", "get_workshop_dashboard", "get_vehicle_portfolio",
        "list_employees", "get_employee_kpi", "get_staff_performance",
        "get_warehouse_report", "get_revenue_analysis",
        "get_supplier_debt_detail", "list_expenses", "list_purchase_invoices",
        "get_active_shipments", "get_logistics_dashboard", "get_fulfillment_orders", "calculate_shipping_fee",
        "create_purchase_request", "list_news", "get_debt_logs_missing_proofs", "get_conversion_tools",
        "get_payroll_summary", "get_commission_records", "get_store_settings", "list_users_and_roles",
        "get_ga4_traffic",
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


def test_summarize_result_khong_loi_cho_moi_tool_trong_catalog():
    fake_envelope = {"items": [{"a": 1, "b": "x"}], "totalCount": 1, "warnings": []}
    for entry in load_catalog():
        summary = summarize_result(entry["name"], fake_envelope)
        assert isinstance(summary, str) and summary


def test_summarize_result_bao_loi_ro_rang_khi_tool_that_bai():
    assert summarize_result("get_sales_summary", {"error": "timeout"}) == "Không lấy được dữ liệu"


def test_tool_tai_chinh_co_summarizer_rieng_khong_roi_vao_generic():
    for name in ("get_sales_summary", "get_product_stock", "get_pnl_report", "get_order_status"):
        assert name in SUMMARIZERS


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
            tool_calls=[_tool_call("get_order_status", {"keyword": "Nguyễn Văn A"}, "call-1")],
        )],
    })
    result = await manager_agent.call_tools_node(state)

    assert result["tool_turns"] == 1
    assert result["tool_limit_reached"] is False
    tool_message = result["messages"][0]
    payload = json.loads(tool_message.content)
    assert payload["items"][0]["tool_path"] == "orders/status"
    assert payload["items"][0]["payload"] == {"keyword": "Nguyễn Văn A"}
    assert tool_message.tool_call_id == "call-1"
    tool_start_type, tool_start_payload = events[0]
    assert tool_start_type == "tool_start"
    start_payload = json.loads(tool_start_payload)
    assert start_payload["name"] == "get_order_status"
    assert start_payload["summary"] == "Từ khoá: Nguyễn Văn A"
    assert isinstance(start_payload["argsPreview"]["preview"], str)

    tool_end_type, tool_end_payload = events[1]
    assert tool_end_type == "tool_end"
    end_payload = json.loads(tool_end_payload)
    assert end_payload["name"] == "get_order_status"
    assert end_payload["truncated"] is False
    assert end_payload["totalCount"] == 1
    assert end_payload["asOf"] == "2026-07-26T09:15:00+07:00"
    assert end_payload["warnings"] == []
    assert end_payload["filtersApplied"] == {}
    assert isinstance(end_payload["durationMs"], int)
    assert end_payload["summary"] == "1 đơn hàng, mới nhất: không rõ"
    assert "resultPreview" in end_payload


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
