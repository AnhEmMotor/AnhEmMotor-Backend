from app.services.backend_client import BackendClient
from app.services.chat_tools import build_all_tools, build_tools, load_catalog

GENERIC_NAMES = {
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

}

def test_load_catalog_doc_dung_file_chung_voi_dotnet():
    catalog = load_catalog()
    names = {entry["name"] for entry in catalog}
    assert names == GENERIC_NAMES
    assert all(entry.get("label") for entry in catalog)
    assert all(entry.get("path") for entry in catalog)


def test_build_tools_khong_bi_le_catalog():
    client = BackendClient("Bearer x")
    tools = build_tools(client)
    assert {t.name for t in tools} == GENERIC_NAMES


def test_build_all_tools_bang_build_tools(monkeypatch):
    client = BackendClient("Bearer x")
    tools = build_all_tools(client)
    assert {t.name for t in tools} == GENERIC_NAMES
