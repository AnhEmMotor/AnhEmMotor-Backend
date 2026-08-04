using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Features.ChatTools.Queries.GetProductDetailForChat;
using Application.Features.ChatTools.Queries.GetProductPriceListForChat;
using Application.Features.ChatTools.Queries.GetProductStockForChat;
using Application.Features.ChatTools.Queries.ListBrandsForChat;
using Application.Features.ChatTools.Queries.SearchProductsForChat;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebAPI.Attributes;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers;

/// <summary>
/// 5 tool đọc dữ liệu công khai-an toàn cho persona "store" của sidecar AI (khách vãng lai, không đăng nhập). Không có
/// [Authorize]/[HasPermission] — không có nhân viên đứng sau request này, mọi action ở đây là dữ liệu public-safe theo
/// thiết kế. KHÔNG thêm action nào ngoài danh sách 5 tool ở Stage 02, kể cả khi tiện tay copy từ
/// InternalChatToolsController — mỗi action mới phải tự hỏi "lộ cho khách xem có sao không".
/// </summary>
[Route("internal/chat/tools/store")]
[AllowAnonymous]
[LocalhostOnly]
[DisableRateLimiting]
public class PublicChatToolsController(ISender sender) : ApiController
{
    [HttpPost("products/search")]
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

    [HttpPost("products/detail")]
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

    /// <summary>
    /// Không trả số lượng tồn kho chính xác — chỉ trả trạng thái con_hang/sap_het/het_hang.
    /// </summary>
    [HttpPost("products/stock")]
    public async Task<IActionResult> GetProductStock(
        [FromBody] GetProductStockForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductStockForChatQuery { ProductId = request.ProductId, Limit = request.Limit },
            cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return HandleResult(Result<ChatToolEnvelope<ChatProductStockPublicDto>>.Failure(result.Errors));
        }
        var envelope = result.Value;
        var publicEnvelope = new ChatToolEnvelope<ChatProductStockPublicDto>(
            envelope.Items.Select(ChatProductStockPublicDto.FromInternal).ToList(),
            envelope.TotalCount,
            envelope.Truncated,
            envelope.AsOf,
            envelope.Timezone,
            envelope.Source,
            envelope.FiltersApplied,
            envelope.Definition,
            null,
            envelope.Warnings);
        return HandleResult(Result<ChatToolEnvelope<ChatProductStockPublicDto>>.Success(publicEnvelope));
    }

    [HttpPost("products/price-list")]
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
    public async Task<IActionResult> ListBrands(
        [FromBody] ListBrandsForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListBrandsForChatQuery { Limit = request.Limit }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }
}
