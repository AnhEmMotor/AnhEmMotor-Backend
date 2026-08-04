using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories.Output;
using Domain.Entities;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Features.ChatTools.Common;

/// <summary>
/// Tìm đơn hàng theo tên khách hàng hoặc SĐT (keyword) — dùng chung cho các chat tool cần tra cứu đơn hàng theo khách
/// thay vì theo Id.
/// </summary>
public static class ChatToolOrderSearch
{
    public const int MaxResults = 5;

    public static async Task<List<int>> FindOrderIdsByKeywordAsync(
        IOutputReadRepository outputReadRepository,
        string keyword,
        CancellationToken cancellationToken)
    {
        Expression<Func<Output, bool>> filter = o => (o.CustomerName != null && o.CustomerName.Contains(keyword)) ||
            (o.CustomerPhone != null && o.CustomerPhone.Contains(keyword));
        var sieveModel = new SieveModel { Sorts = "-CreatedAt", Page = 1, PageSize = MaxResults };
        var paged = await outputReadRepository
            .GetPagedAsync<OutputItemResponse>(sieveModel, filter: filter, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return (paged.Items ?? []).Select(i => i.Id).ToList();
    }
}
