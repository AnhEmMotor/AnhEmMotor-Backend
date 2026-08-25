using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Marketing.Queries.GetProductViewHistory;

public class GetProductViewHistoryQuery : IRequest<Result<PagedResult<ProductViewHistoryResponse>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; } // Could be visitor key or customer name

    /// <summary>Lọc theo khoảng thời gian truy cập (ViewedAt). Trống = lấy tất cả.</summary>
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}
