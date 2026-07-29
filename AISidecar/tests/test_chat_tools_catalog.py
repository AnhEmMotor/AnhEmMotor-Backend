from app.services.backend_client import BackendClient
from app.services.chat_tools import build_tools, load_catalog

EXPECTED_NAMES = {
    "search_products", "get_product_stock", "get_low_stock_products",
    "get_order_status", "get_sales_summary", "get_top_selling",
}


def test_load_catalog_doc_dung_file_chung_voi_dotnet():
    catalog = load_catalog()
    names = {entry["name"] for entry in catalog}
    assert names == EXPECTED_NAMES
    assert all(entry.get("label") for entry in catalog)
    assert all(entry.get("path") for entry in catalog)


def test_build_tools_khong_bi_le_catalog():
    client = BackendClient("Bearer x")
    tools = build_tools(client)
    assert {t.name for t in tools} == EXPECTED_NAMES
