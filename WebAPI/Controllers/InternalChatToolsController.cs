using Application.Features.ChatTools.Commands.CreatePurchaseRequestForChat;
using Application.Features.ChatTools.Queries.CalculateShippingFeeForChat;
using Application.Features.ChatTools.Queries.GetActiveShipmentsForChat;
using Application.Features.ChatTools.Queries.GetCommissionRecordsForChat;
using Application.Features.ChatTools.Queries.GetConversionToolsForChat;
using Application.Features.ChatTools.Queries.GetCustomerProfileForChat;
using Application.Features.ChatTools.Queries.GetDashboardOverviewForChat;
using Application.Features.ChatTools.Queries.GetDebtLogsMissingProofsForChat;
using Application.Features.ChatTools.Queries.GetEmployeeKpiForChat;
using Application.Features.ChatTools.Queries.GetFulfillmentOrdersForChat;
using Application.Features.ChatTools.Queries.GetInventoryLedgerForChat;
using Application.Features.ChatTools.Queries.GetInventoryReceiptDetailForChat;
using Application.Features.ChatTools.Queries.GetInventoryReportForChat;
using Application.Features.ChatTools.Queries.GetLeadDetailForChat;
using Application.Features.ChatTools.Queries.GetLeadPipelineForChat;
using Application.Features.ChatTools.Queries.GetLogisticsDashboardForChat;
using Application.Features.ChatTools.Queries.GetLowStockProductsForChat;
using Application.Features.ChatTools.Queries.GetLoyaltyMembersForChat;
using Application.Features.ChatTools.Queries.GetOrderStatisticsForChat;
using Application.Features.ChatTools.Queries.GetOrderStatusForChat;
using Application.Features.ChatTools.Queries.GetPayrollSummaryForChat;
using Application.Features.ChatTools.Queries.GetPnlReportForChat;
using Application.Features.ChatTools.Queries.GetProductDetailForChat;
using Application.Features.ChatTools.Queries.GetProductPriceListForChat;
using Application.Features.ChatTools.Queries.GetProductStockForChat;
using Application.Features.ChatTools.Queries.GetPurchaseRequestDetailForChat;
using Application.Features.ChatTools.Queries.GetRecentTransactionsForChat;
using Application.Features.ChatTools.Queries.GetRepairOrderDetailForChat;
using Application.Features.ChatTools.Queries.GetRevenueAnalysisForChat;
using Application.Features.ChatTools.Queries.GetRevenueByCategoryForChat;
using Application.Features.ChatTools.Queries.GetSalesReportForChat;
using Application.Features.ChatTools.Queries.GetSalesSummaryForChat;
using Application.Features.ChatTools.Queries.GetShipmentTrackingForChat;
using Application.Features.ChatTools.Queries.GetStaffPerformanceForChat;
using Application.Features.ChatTools.Queries.GetStoreSettingsForChat;
using Application.Features.ChatTools.Queries.GetSupplierDebtDetailForChat;
using Application.Features.ChatTools.Queries.GetSupplierPricesForVariantForChat;
using Application.Features.ChatTools.Queries.GetSupplierStatisticsForChat;
using Application.Features.ChatTools.Queries.GetSuppliersWithDebtForChat;
using Application.Features.ChatTools.Queries.GetTopSellingForChat;
using Application.Features.ChatTools.Queries.GetVehiclePortfolioForChat;
using Application.Features.ChatTools.Queries.GetWarehouseReportForChat;
using Application.Features.ChatTools.Queries.GetWarrantyClaimDetailForChat;
using Application.Features.ChatTools.Queries.GetWarrantyTermsForChat;
using Application.Features.ChatTools.Queries.GetWorkshopDashboardForChat;
using Application.Features.ChatTools.Queries.ListBookingAppointmentsForChat;
using Application.Features.ChatTools.Queries.ListBookingsForChat;
using Application.Features.ChatTools.Queries.ListBrandsForChat;
using Application.Features.ChatTools.Queries.ListCategoriesForChat;
using Application.Features.ChatTools.Queries.ListContactsForChat;
using Application.Features.ChatTools.Queries.ListEmployeesForChat;
using Application.Features.ChatTools.Queries.ListExpensesForChat;
using Application.Features.ChatTools.Queries.ListFinanceContractsForChat;
using Application.Features.ChatTools.Queries.ListInventoryReceiptsForChat;
using Application.Features.ChatTools.Queries.ListNewsForChat;
using Application.Features.ChatTools.Queries.ListOrdersForChat;
using Application.Features.ChatTools.Queries.ListPurchaseInvoicesForChat;
using Application.Features.ChatTools.Queries.ListPurchaseRequestsForChat;
using Application.Features.ChatTools.Queries.ListRepairOrdersForChat;
using Application.Features.ChatTools.Queries.ListSalesContractsForChat;
using Application.Features.ChatTools.Queries.ListServicesForChat;
using Application.Features.ChatTools.Queries.ListSupplierContractsForChat;
using Application.Features.ChatTools.Queries.ListUsersAndRolesForChat;
using Application.Features.ChatTools.Queries.ListVouchersForChat;
using Application.Features.ChatTools.Queries.ListWarrantyClaimsForChat;
using Application.Features.ChatTools.Queries.ListWorkshopPaymentsForChat;
using Application.Features.ChatTools.Queries.SearchCustomersForChat;
using Application.Features.ChatTools.Queries.SearchProductsForChat;
using Application.Features.ChatTools.Queries.SearchSuppliersForChat;
using Application.Interfaces.Services;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers;

/// <summary>
/// Các tool đọc dữ liệu thật (sản phẩm, tồn kho, đơn hàng, doanh thu) cho AI sidecar gọi trong luồng tool-calling. Mỗi
/// action tự kiểm tra permission độc lập với những gì sidecar/prompt tuyên bố.
/// </summary>
[Route("internal/chat/tools")]
[Authorize]
[Attributes.LocalhostOnly]
[DisableRateLimiting]
public class InternalChatToolsController(ISender sender, IChatToolCatalogProvider catalogProvider) : ApiController
{
    /// <summary>
    /// Kiểm kê tool đang active + build id, dùng để sidecar tự đối chiếu hợp đồng lúc khởi động (Stage 17.5).
    /// AllowAnonymous vì sidecar gọi lúc chưa có phiên user nào — LocalhostOnly là hàng rào thật.
    /// </summary>
    [HttpGet("manifest")]
    [AllowAnonymous]
    public IActionResult GetManifest()
    {
        var tools = catalogProvider.GetCatalog().Where(t => t.Status == "active").Select(t => t.Name).ToList();
        var buildId = typeof(InternalChatToolsController).Assembly.GetName().Version?.ToString() ?? "dev";
        return Ok(new { tools, buildId });
    }

    [HttpPost("products/search")]
    [HasPermission(Permissions.Order.ProductManagement.View)]
    public async Task<IActionResult> SearchProducts(
        [FromBody] SearchProductsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchProductsForChatQuery { Keyword = request.Keyword, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("products/stock")]
    [HasPermission(Permissions.Warehouse.ProductManagement.View)]
    public async Task<IActionResult> GetProductStock(
        [FromBody] GetProductStockForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductStockForChatQuery { ProductId = request.ProductId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("products/low-stock")]
    [HasPermission(Permissions.Warehouse.ProductManagement.View)]
    public async Task<IActionResult> GetLowStockProducts(
        [FromBody] GetLowStockProductsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLowStockProductsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("orders/status")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> GetOrderStatus(
        [FromBody] GetOrderStatusForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderStatusForChatQuery { Keyword = request.Keyword }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("analytics/sales")]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    public async Task<IActionResult> GetSalesSummary(
        [FromBody] GetSalesSummaryForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSalesSummaryForChatQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("analytics/top-selling")]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    public async Task<IActionResult> GetTopSelling(
        [FromBody] GetTopSellingForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTopSellingForChatQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("products/detail")]
    [HasPermission(Permissions.Order.ProductManagement.View)]
    public async Task<IActionResult> GetProductDetail(
        [FromBody] GetProductDetailForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductDetailForChatQuery { ProductId = request.ProductId },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("inventory/report")]
    [HasPermission(Permissions.Warehouse.InventoryReportManagement.View)]
    public async Task<IActionResult> GetInventoryReport(
        [FromBody] GetInventoryReportForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetInventoryReportForChatQuery
            {
                Limit = request.Limit,
                SearchTerm = request.SearchTerm,
                Month = request.Month,
                Year = request.Year
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("orders/list")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> ListOrders(
        [FromBody] ListOrdersForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListOrdersForChatQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                StatusId = request.StatusId,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("orders/statistics")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> GetOrderStatistics(
        [FromBody] GetOrderStatisticsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOrderStatisticsForChatQuery { FromDate = request.FromDate, ToDate = request.ToDate },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("customers/profile")]
    [HasPermission(Permissions.Order.CustomerManagement.View)]
    public async Task<IActionResult> GetCustomerProfile(
        [FromBody] GetCustomerProfileForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCustomerProfileForChatQuery { CustomerId = request.CustomerId },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("customers/search")]
    [HasPermission(Permissions.Order.CustomerManagement.View)]
    public async Task<IActionResult> SearchCustomers(
        [FromBody] SearchCustomersForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchCustomersForChatQuery { Keyword = request.Keyword, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("repair-orders/list")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    public async Task<IActionResult> ListRepairOrders(
        [FromBody] ListRepairOrdersForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListRepairOrdersForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("repair-orders/detail")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    public async Task<IActionResult> GetRepairOrderDetail(
        [FromBody] GetRepairOrderDetailForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetRepairOrderDetailForChatQuery { Keyword = request.Keyword },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("warranty/claims")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    public async Task<IActionResult> ListWarrantyClaims(
        [FromBody] ListWarrantyClaimsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListWarrantyClaimsForChatQuery { StatusId = request.StatusId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("finance/pnl")]
    [HasPermission(Permissions.Accountant.DashboardManagement.View)]
    public async Task<IActionResult> GetPnlReport(
        [FromBody] GetPnlReportForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetPnlReportForChatQuery { FromDate = request.FromDate, ToDate = request.ToDate },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("finance/suppliers-with-debt")]
    [HasPermission(Permissions.Accountant.DebtPaymentManagement.View)]
    public async Task<IActionResult> GetSuppliersWithDebt(
        [FromBody] GetSuppliersWithDebtForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSuppliersWithDebtForChatQuery { Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("logistics/shipment-tracking")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> GetShipmentTracking(
        [FromBody] GetShipmentTrackingForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetShipmentTrackingForChatQuery { Keyword = request.Keyword },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("admin/dashboard-overview")]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    public async Task<IActionResult> GetDashboardOverview(
        [FromBody] GetDashboardOverviewForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDashboardOverviewForChatQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("bookings/appointments")]
    [HasPermission(Permissions.Factory.BookingAppointmentManagement.View)]
    public async Task<IActionResult> ListBookingAppointments(
        [FromBody] ListBookingAppointmentsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListBookingAppointmentsForChatQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("products/price-list")]
    [HasPermission(Permissions.Order.ProductManagement.View)]
    public async Task<IActionResult> GetProductPriceList(
        [FromBody] GetProductPriceListForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductPriceListForChatQuery { Keyword = request.Keyword, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("products/brands")]
    [HasPermission(Permissions.Warehouse.ProductManagement.View)]
    public async Task<IActionResult> ListBrands(
        [FromBody] ListBrandsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListBrandsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("products/categories")]
    [HasPermission(Permissions.Warehouse.ProductManagement.View)]
    public async Task<IActionResult> ListCategories(
        [FromBody] ListCategoriesForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListCategoriesForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("suppliers/variant-prices")]
    [HasPermission(Permissions.Warehouse.SupplierManagement.View)]
    public async Task<IActionResult> GetSupplierPricesForVariant(
        [FromBody] GetSupplierPricesForVariantForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSupplierPricesForVariantForChatQuery { VariantId = request.VariantId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("suppliers/search")]
    [HasPermission(Permissions.Warehouse.SupplierManagement.View)]
    public async Task<IActionResult> SearchSuppliers(
        [FromBody] SearchSuppliersForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchSuppliersForChatQuery { Keyword = request.Keyword, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("suppliers/statistics")]
    [HasPermission(Permissions.Warehouse.SupplierManagement.View)]
    public async Task<IActionResult> GetSupplierStatistics(
        [FromBody] GetSupplierStatisticsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSupplierStatisticsForChatQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("inventory/ledger")]
    [HasPermission(Permissions.Warehouse.LedgerManagement.View)]
    public async Task<IActionResult> GetInventoryLedger(
        [FromBody] GetInventoryLedgerForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetInventoryLedgerForChatQuery
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("inventory/receipts")]
    [HasPermission(Permissions.Warehouse.ReceiptManagement.View)]
    public async Task<IActionResult> ListInventoryReceipts(
        [FromBody] ListInventoryReceiptsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListInventoryReceiptsForChatQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("inventory/receipts/detail")]
    [HasPermission(Permissions.Warehouse.ReceiptManagement.View)]
    public async Task<IActionResult> GetInventoryReceiptDetail(
        [FromBody] GetInventoryReceiptDetailForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetInventoryReceiptDetailForChatQuery { Keyword = request.Keyword },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("inventory/purchase-requests")]
    [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.View)]
    public async Task<IActionResult> ListPurchaseRequests(
        [FromBody] ListPurchaseRequestsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListPurchaseRequestsForChatQuery { StatusId = request.StatusId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("inventory/purchase-requests/detail")]
    [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.View)]
    public async Task<IActionResult> GetPurchaseRequestDetail(
        [FromBody] GetPurchaseRequestDetailForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetPurchaseRequestDetailForChatQuery { Keyword = request.Keyword },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("analytics/revenue-by-category")]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    public async Task<IActionResult> GetRevenueByCategory(
        [FromBody] GetRevenueByCategoryForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetRevenueByCategoryForChatQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("sales/report")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> GetSalesReport(
        [FromBody] GetSalesReportForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSalesReportForChatQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("analytics/recent-transactions")]
    [HasPermission(Permissions.Accountant.DashboardManagement.View)]
    public async Task<IActionResult> GetRecentTransactions(
        [FromBody] GetRecentTransactionsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetRecentTransactionsForChatQuery { Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("contracts/sales")]
    [HasPermission(Permissions.Order.ContractManagement.View)]
    public async Task<IActionResult> ListSalesContracts(
        [FromBody] ListSalesContractsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListSalesContractsForChatQuery { StatusId = request.StatusId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("contracts/finance")]
    [HasPermission(Permissions.Admin.FinanceContractManagement.View)]
    public async Task<IActionResult> ListFinanceContracts(
        [FromBody] ListFinanceContractsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListFinanceContractsForChatQuery { StatusId = request.StatusId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("contracts/supplier")]
    [HasPermission(Permissions.Accountant.SupplierContractManagement.View)]
    public async Task<IActionResult> ListSupplierContracts(
        [FromBody] ListSupplierContractsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListSupplierContractsForChatQuery { StatusId = request.StatusId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("marketing/vouchers")]
    [HasPermission(Permissions.Marketing.CustomerVoucherManagement.View)]
    public async Task<IActionResult> ListVouchers(
        [FromBody] ListVouchersForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListVouchersForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("leads/pipeline")]
    [HasPermission(Permissions.Marketing.LeadManagement.View)]
    public async Task<IActionResult> GetLeadPipeline(
        [FromBody] GetLeadPipelineForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLeadPipelineForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("leads/detail")]
    [HasPermission(Permissions.Marketing.LeadManagement.View)]
    public async Task<IActionResult> GetLeadDetail(
        [FromBody] GetLeadDetailForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLeadDetailForChatQuery { Keyword = request.Keyword }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("contacts/list")]
    [HasPermission(Permissions.Marketing.ContactManagement.View)]
    public async Task<IActionResult> ListContacts(
        [FromBody] ListContactsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListContactsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("loyalty/members")]
    [HasPermission(Permissions.Marketing.CustomerCareManagement.View)]
    public async Task<IActionResult> GetLoyaltyMembers(
        [FromBody] GetLoyaltyMembersForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLoyaltyMembersForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("warranty/claims/detail")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    public async Task<IActionResult> GetWarrantyClaimDetail(
        [FromBody] GetWarrantyClaimDetailForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetWarrantyClaimDetailForChatQuery { Keyword = request.Keyword },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("warranty/terms")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    public async Task<IActionResult> GetWarrantyTerms(
        [FromBody] GetWarrantyTermsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWarrantyTermsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("workshop/payments")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    public async Task<IActionResult> ListWorkshopPayments(
        [FromBody] ListWorkshopPaymentsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListWorkshopPaymentsForChatQuery { Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("bookings/list")]
    [HasPermission(Permissions.Factory.BookingManagement.View)]
    public async Task<IActionResult> ListBookings(
        [FromBody] ListBookingsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListBookingsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("services/list")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    public async Task<IActionResult> ListServices(
        [FromBody] ListServicesForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListServicesForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("workshop/dashboard")]
    [HasPermission(Permissions.Factory.DashboardManagement.View)]
    public async Task<IActionResult> GetWorkshopDashboard(
        [FromBody] GetWorkshopDashboardForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetWorkshopDashboardForChatQuery { FromDate = request.FromDate, ToDate = request.ToDate },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("vehicles/portfolio")]
    [HasPermission(Permissions.Factory.CustomerManagement.View)]
    public async Task<IActionResult> GetVehiclePortfolio(
        [FromBody] GetVehiclePortfolioForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetVehiclePortfolioForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("hr/employees")]
    [HasPermission(Permissions.Admin.EmployeeManagement.View)]
    public async Task<IActionResult> ListEmployees(
        [FromBody] ListEmployeesForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListEmployeesForChatQuery { Keyword = request.Keyword, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("hr/employee-kpi")]
    [HasPermission(Permissions.Admin.EmployeeManagement.View)]
    public async Task<IActionResult> GetEmployeeKpi(
        [FromBody] GetEmployeeKpiForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmployeeKpiForChatQuery(request.EmployeeId), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("hr/staff-performance")]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    public async Task<IActionResult> GetStaffPerformance(
        [FromBody] GetStaffPerformanceForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetStaffPerformanceForChatQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("admin/warehouse-report")]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    public async Task<IActionResult> GetWarehouseReport(
        [FromBody] GetWarehouseReportForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetWarehouseReportForChatQuery { FromDate = request.FromDate, ToDate = request.ToDate },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("admin/revenue-analysis")]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    public async Task<IActionResult> GetRevenueAnalysis(
        [FromBody] GetRevenueAnalysisForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetRevenueAnalysisForChatQuery { FromDate = request.FromDate, ToDate = request.ToDate },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("finance/supplier-debt-detail")]
    [HasPermission(Permissions.Accountant.DebtPaymentManagement.View)]
    public async Task<IActionResult> GetSupplierDebtDetail(
        [FromBody] GetSupplierDebtDetailForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSupplierDebtDetailForChatQuery { SupplierId = request.SupplierId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("finance/expenses")]
    [HasPermission(Permissions.Accountant.DashboardManagement.View)]
    public async Task<IActionResult> ListExpenses(
        [FromBody] ListExpensesForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListExpensesForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("finance/purchase-invoices")]
    [HasPermission(Permissions.Warehouse.ReceiptManagement.View)]
    public async Task<IActionResult> ListPurchaseInvoices(
        [FromBody] ListPurchaseInvoicesForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListPurchaseInvoicesForChatQuery { Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("logistics/active-shipments")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> GetActiveShipments(
        [FromBody] GetActiveShipmentsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetActiveShipmentsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("logistics/dashboard")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> GetLogisticsDashboard(
        [FromBody] GetLogisticsDashboardForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetLogisticsDashboardForChatQuery { Range = request.Range },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("logistics/fulfillment-orders")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> GetFulfillmentOrders(
        [FromBody] GetFulfillmentOrdersForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetFulfillmentOrdersForChatQuery
            {
                Status = request.Status,
                Carrier = request.Carrier,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("logistics/shipping-fee")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> CalculateShippingFee(
        [FromBody] CalculateShippingFeeForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CalculateShippingFeeForChatQuery
            {
                ProvinceId = request.ProvinceId,
                WardId = request.WardId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("inventory/purchase-requests/create")]
    [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Create)]
    public async Task<IActionResult> CreatePurchaseRequest(
        [FromBody] CreatePurchaseRequestForChatRequest request,
        CancellationToken cancellationToken)
    {
        var items = JsonSerializer.Deserialize<List<ChatCreatePurchaseRequestItemInput>>(
                request.ItemsJson,
                JsonSerializerOptions.Web) ??
            [];
        var result = await sender.Send(
            new CreatePurchaseRequestForChatCommand { Items = items, Note = request.Note },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("marketing/news")]
    [HasPermission(Permissions.Marketing.NewsManagement.View)]
    public async Task<IActionResult> ListNews(
        [FromBody] ListNewsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListNewsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("finance/debt-logs-missing-proofs")]
    [HasPermission(Permissions.Accountant.DebtPaymentManagement.View)]
    public async Task<IActionResult> GetDebtLogsMissingProofs(
        [FromBody] GetDebtLogsMissingProofsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetDebtLogsMissingProofsForChatQuery { Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("knowledge/conversion-tools")]
    public async Task<IActionResult> GetConversionTools(
        [FromBody] GetConversionToolsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetConversionToolsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("hr/payroll-summary")]
    [HasPermission(Permissions.Admin.PayrollManagement.View)]
    public async Task<IActionResult> GetPayrollSummary(
        [FromBody] GetPayrollSummaryForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetPayrollSummaryForChatQuery
            {
                EmployeeId = request.EmployeeId,
                Month = request.Month,
                Year = request.Year,
                Limit = request.Limit
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("hr/commission-records")]
    [HasPermission(Permissions.Admin.PayrollManagement.View)]
    public async Task<IActionResult> GetCommissionRecords(
        [FromBody] GetCommissionRecordsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCommissionRecordsForChatQuery { EmployeeId = request.EmployeeId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("admin/store-settings")]
    [HasPermission(Permissions.Admin.SettingManagement.View)]
    public async Task<IActionResult> GetStoreSettings(
        [FromBody] GetStoreSettingsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreSettingsForChatQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("admin/users-and-roles")]
    [HasPermission(Permissions.Admin.UserManagement.View)]
    public async Task<IActionResult> ListUsersAndRoles(
        [FromBody] ListUsersAndRolesForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListUsersAndRolesForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("marketing/product-view-history")]
    [HasPermission(Permissions.Marketing.CustomerCareManagement.View)]
    public async Task<IActionResult> GetProductViewHistory(
        [FromBody] Application.Features.Marketing.Queries.GetProductViewHistoryForChat.GetProductViewHistoryForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new Application.Features.Marketing.Queries.GetProductViewHistoryForChat.GetProductViewHistoryForChatQuery(request.VisitorKey, request.CustomerId, request.Limit),
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }
}

public record CreatePurchaseRequestForChatRequest
{
    public string ItemsJson { get; init; } = "[]";

    public string? Note { get; init; }
}

public record ListNewsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetDebtLogsMissingProofsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetConversionToolsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetPayrollSummaryForChatRequest
{
    public int? EmployeeId { get; init; }

    public int? Month { get; init; }

    public int? Year { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetCommissionRecordsForChatRequest
{
    public int? EmployeeId { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetStoreSettingsForChatRequest
{
}

public record ListUsersAndRolesForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetActiveShipmentsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetLogisticsDashboardForChatRequest
{
    public string Range { get; init; } = "today";
}

public record GetFulfillmentOrdersForChatRequest
{
    public string? Status { get; init; }

    public string? Carrier { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}

public record CalculateShippingFeeForChatRequest
{
    public int ProvinceId { get; init; }

    public string WardId { get; init; } = string.Empty;

    public int ProductVariantId { get; init; }

    public int Quantity { get; init; } = 1;
}

public record GetSupplierDebtDetailForChatRequest
{
    public int SupplierId { get; init; }

    public int Limit { get; init; } = 10;
}

public record ListExpensesForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record ListPurchaseInvoicesForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetProductPriceListForChatRequest
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = 10;
}

public record ListBrandsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record ListCategoriesForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetSupplierPricesForVariantForChatRequest
{
    public int VariantId { get; init; }

    public int Limit { get; init; } = 10;
}

public record SearchSuppliersForChatRequest
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetSupplierStatisticsForChatRequest
{
}

public record GetInventoryLedgerForChatRequest
{
    public int? ProductId { get; init; }

    public int? VariantId { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}

public record ListInventoryReceiptsForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetInventoryReceiptDetailForChatRequest
{
    public required string Keyword { get; init; }
}

public record ListPurchaseRequestsForChatRequest
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetPurchaseRequestDetailForChatRequest
{
    public required string Keyword { get; init; }
}

public record GetRevenueByCategoryForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetSalesReportForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetRecentTransactionsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record ListSalesContractsForChatRequest
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = 10;
}

public record ListFinanceContractsForChatRequest
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = 10;
}

public record ListSupplierContractsForChatRequest
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = 10;
}

public record ListVouchersForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetLeadPipelineForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetLeadDetailForChatRequest
{
    public required string Keyword { get; init; }
}

public record ListContactsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetLoyaltyMembersForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetWarrantyClaimDetailForChatRequest
{
    public required string Keyword { get; init; }
}

public record GetWarrantyTermsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record ListWorkshopPaymentsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record ListBookingsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record ListServicesForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetWorkshopDashboardForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}

public record GetVehiclePortfolioForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record ListEmployeesForChatRequest
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetEmployeeKpiForChatRequest
{
    public int EmployeeId { get; init; }
}

public record GetStaffPerformanceForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetWarehouseReportForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}

public record GetRevenueAnalysisForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}

public record GetProductDetailForChatRequest
{
    public int ProductId { get; init; }
}

public record GetInventoryReportForChatRequest
{
    public int Limit { get; init; } = 10;

    public string? SearchTerm { get; init; }

    public int? Month { get; init; }

    public int? Year { get; init; }
}

public record ListOrdersForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public string? StatusId { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetOrderStatisticsForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}

public record GetCustomerProfileForChatRequest
{
    public int CustomerId { get; init; }
}

public record SearchCustomersForChatRequest
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = 10;
}

public record ListRepairOrdersForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetRepairOrderDetailForChatRequest
{
    public required string Keyword { get; init; }
}

public record ListWarrantyClaimsForChatRequest
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetPnlReportForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}

public record GetSuppliersWithDebtForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetShipmentTrackingForChatRequest
{
    public required string Keyword { get; init; }
}

public record GetDashboardOverviewForChatRequest
{
}

public record ListBookingAppointmentsForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}

public record SearchProductsForChatRequest
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetProductStockForChatRequest
{
    public int ProductId { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetLowStockProductsForChatRequest
{
    public int Limit { get; init; } = 10;
}

public record GetOrderStatusForChatRequest
{
    public required string Keyword { get; init; }
}

public record GetSalesSummaryForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}

public record GetTopSellingForChatRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = 10;
}
