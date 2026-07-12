using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Voucher;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Features.Vouchers.Queries.GetVoucherList;

public class GetVouchersQueryHandler(IVoucherReadRepository readRepository) : IRequestHandler<GetVouchersQuery, Result<PagedResult<VoucherResponse>>>
{
    public async Task<Result<PagedResult<VoucherResponse>>> Handle(
        GetVouchersQuery request,
        CancellationToken cancellationToken)
    {
        var sieveModel = request.Request ?? new SieveModel();
        var search = ExtractFilterValue(sieveModel.Filters, "search");
        Expression<Func<Voucher, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(search))
        {
            filter = v => v.Code.Contains(search) || v.Name.Contains(search);
            sieveModel.Filters = RemoveFilter(sieveModel.Filters, "search");
        }
        if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
        {
            sieveModel.Sorts = $"-{nameof(Voucher.CreatedAt)}";
        }
        var result = await readRepository.GetPagedAsync<VoucherResponse>(
            sieveModel,
            DataFetchMode.ActiveOnly,
            filter,
            cancellationToken)
            .ConfigureAwait(false);
        return Result<PagedResult<VoucherResponse>>.Success(result);
    }

    private static string? ExtractFilterValue(string? filters, string key)
    {
        if (string.IsNullOrWhiteSpace(filters))
            return null;
        var parts = filters.Split(',');
        foreach (var part in parts)
        {
            var keyValue = part.Split(['=', '@', '!', '<', '>'], 2);
            if (keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                var value = keyValue[1].Trim();
                return value.TrimStart('=', '@', '!', '<', '>', '*');
            }
        }
        return null;
    }

    private static string? RemoveFilter(string? filters, string key)
    {
        if (string.IsNullOrWhiteSpace(filters))
            return filters;
        var parts = filters.Split(',').ToList();
        parts.RemoveAll(p => p.TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase));
        return string.Join(",", parts);
    }
}
