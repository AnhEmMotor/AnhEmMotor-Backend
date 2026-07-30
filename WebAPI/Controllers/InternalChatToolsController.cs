using Application.Common.Models;
using Application.Features.ChatTools.Queries.GetLowStockProductsForChat;
using Application.Features.ChatTools.Queries.GetOrderStatusForChat;
using Application.Features.ChatTools.Queries.GetProductStockForChat;
using Application.Features.ChatTools.Queries.GetSalesSummaryForChat;
using Application.Features.ChatTools.Queries.GetTopSellingForChat;
using Application.Features.ChatTools.Queries.SearchProductsForChat;
using Application.Interfaces.Services;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers;

/// <summary>
/// Các tool đọc dữ liệu thật (sản phẩm, tồn kho, đơn hàng, doanh thu) cho AI sidecar gọi trong luồng tool-calling.
/// Mỗi action tự kiểm tra permission độc lập với những gì sidecar/prompt tuyên bố.
/// </summary>
[Route("internal/chat/tools")]
[Authorize]
[WebAPI.Attributes.LocalhostOnly]
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
        var tools = catalogProvider.GetCatalog()
            .Where(t => t.Status == "active")
            .Select(t => t.Name)
            .ToList();
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
        var result = await sender.Send(
            new GetLowStockProductsForChatQuery { Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("orders/status")]
    [HasPermission(Permissions.Order.OrderManagement.View)]
    public async Task<IActionResult> GetOrderStatus(
        [FromBody] GetOrderStatusForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderStatusForChatQuery { OrderId = request.OrderId }, cancellationToken)
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
            new GetSalesSummaryForChatQuery { FromDate = request.FromDate, ToDate = request.ToDate, Limit = request.Limit },
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
            new GetTopSellingForChatQuery { FromDate = request.FromDate, ToDate = request.ToDate, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

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
    public int OrderId { get; init; }
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
