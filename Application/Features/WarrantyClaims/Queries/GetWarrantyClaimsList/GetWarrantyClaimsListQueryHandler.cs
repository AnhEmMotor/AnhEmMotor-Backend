using Application.ApiContracts.WarrantyClaim.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyClaim;
using Domain.Enums;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WarrantyClaims.Queries.GetWarrantyClaimsList
{
    public class GetWarrantyClaimsListQueryHandler(IWarrantyClaimReadRepository warrantyClaimReadRepository)
        : IRequestHandler<GetWarrantyClaimsListQuery, Result<PagedResult<WarrantyClaimResponse>>>
    {
        public async Task<Result<PagedResult<WarrantyClaimResponse>>> Handle(
            GetWarrantyClaimsListQuery request,
            CancellationToken cancellationToken)
        {
            var sieveModel = request.SieveModel ?? new SieveModel();
            var search = ExtractFilterValue(sieveModel.Filters, "search");
            Expression<Func<WarrantyClaim, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = w => (w.ClaimNumber.Contains(search)) ||
                    (w.Vehicle != null && w.Vehicle.LicensePlate.Contains(search)) ||
                    (w.Vehicle != null && w.Vehicle.User != null && w.Vehicle.User.FullName.Contains(search));
                sieveModel.Filters = RemoveFilter(sieveModel.Filters, "search");
            }
            if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                sieveModel.Sorts = $"-{nameof(WarrantyClaim.CreatedAt)}";
            }
            var result = await warrantyClaimReadRepository.GetPagedAsync<WarrantyClaimResponse>(
                sieveModel,
                Domain.Constants.DataFetchMode.ActiveOnly,
                filter,
                cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        private static string? ExtractFilterValue(string? filters, string key)
        {
            if (string.IsNullOrWhiteSpace(filters))
            {
                return null;
            }
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
            {
                return filters;
            }
            var parts = filters.Split(',').ToList();
            parts.RemoveAll(
                p => p.Split(['=', '@', '!', '<', '>'], 2)[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase));
            return string.Join(",", parts);
        }
    }
}
