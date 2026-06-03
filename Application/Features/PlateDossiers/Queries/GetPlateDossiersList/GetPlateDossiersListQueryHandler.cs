using Application.ApiContracts.PlateDossier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PlateDossier;
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

namespace Application.Features.PlateDossiers.Queries.GetPlateDossiersList
{
    public class GetPlateDossiersListQueryHandler(
        IPlateDossierReadRepository plateDossierReadRepository) : IRequestHandler<GetPlateDossiersListQuery, Result<PagedResult<PlateDossierResponse>>>
    {
        public async Task<Result<PagedResult<PlateDossierResponse>>> Handle(GetPlateDossiersListQuery request, CancellationToken cancellationToken)
        {
            var sieveModel = request.SieveModel ?? new SieveModel();
            var search = ExtractFilterValue(sieveModel.Filters, "search");
            Expression<Func<PlateDossier, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = p => p.LicensePlate.Contains(search) ||
                    (p.Output != null && p.Output.CustomerName != null && p.Output.CustomerName.Contains(search)) ||
                    (p.Output != null && p.Output.CustomerPhone != null && p.Output.CustomerPhone.Contains(search));
                sieveModel.Filters = RemoveFilter(sieveModel.Filters, "search");
            }

            if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                sieveModel.Sorts = $"-{nameof(PlateDossier.CreatedAt)}";
            }

            var result = await plateDossierReadRepository.GetPagedAsync<PlateDossierResponse>(
                sieveModel,
                DataFetchMode.ActiveOnly,
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
