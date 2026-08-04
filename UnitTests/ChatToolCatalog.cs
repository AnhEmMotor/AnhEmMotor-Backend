using Application.Common.Models;
using Application.Features.ManagerChat.Queries.GetChatToolCatalog;
using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests;

public class ChatToolCatalog
{
    private static readonly string[] ExpectedNames =
    [
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
        "semantic_product_search", "search_knowledge",
    ];

    [Fact(DisplayName = "CATALOG_01 - Unit - GetChatToolCatalogQueryHandler map đúng Name/Label từ provider")]
    public async Task Handle_MapsProviderEntriesToDtos()
    {
        var providerMock = new Mock<IChatToolCatalogProvider>();
        providerMock.Setup(p => p.GetCatalog()).Returns(
        [
            new ChatToolCatalogEntry("search_products", "products/search", "Tìm sản phẩm"),
        ]);
        var handler = new GetChatToolCatalogQueryHandler(providerMock.Object);

        var result = await handler.Handle(new GetChatToolCatalogQuery(), CancellationToken.None)
            .ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value![0].Name.Should().Be("search_products");
        result.Value[0].Label.Should().Be("Tìm sản phẩm");
    }

    [Fact(DisplayName = "CATALOG_02 - Guard - chat-tools-catalog.json khớp đúng 71 tool đã implement (Stage 3 + Stage 15 P1+P2+P3 + Stage 12), không lệch tên/route")]
    public void ChatToolCatalogProvider_DocDungFileThat_KhongLech6Tool()
    {
        var provider = new ChatToolCatalogProvider(NullLogger<ChatToolCatalogProvider>.Instance);

        var catalog = provider.GetCatalog();

        catalog.Should().HaveCount(71, "chat-tools-catalog.json phải khớp đúng 71/71 tool Stage 3 + Stage 15 P1+P2+P3 + Stage 12 (semantic_product_search, search_knowledge) đã implement");
        catalog.Select(e => e.Name).Should().BeEquivalentTo(ExpectedNames,
            "thêm/xoá tool phải sửa chat-tools-catalog.json — đây là nguồn duy nhất cho cả .NET và sidecar Python");
        catalog.Should().OnlyContain(e => !string.IsNullOrWhiteSpace(e.Label), "mỗi tool phải có label tiếng Việt cho FE");
        catalog.Should().OnlyContain(e => !string.IsNullOrWhiteSpace(e.Path), "mỗi tool phải có path khớp route InternalChatToolsController");
    }
}
