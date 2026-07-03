using Application.ApiContracts.Maintenance.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MaintenanceHistory;
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

namespace Application.Features.Maintenances.Queries.GetMaintenanceTicketsList
{
    public class GetMaintenanceTicketsListQueryHandler(IMaintenanceHistoryReadRepository repository)
        : IRequestHandler<GetMaintenanceTicketsListQuery, Result<PagedResult<MaintenanceTicketResponse>>>
    {
        public async Task<Result<PagedResult<MaintenanceTicketResponse>>> Handle(
            GetMaintenanceTicketsListQuery request,
            CancellationToken cancellationToken)
        {
            var sieveModel = request.SieveModel ?? new SieveModel();
            var search = ExtractFilterValue(sieveModel.Filters, "search");
            Expression<Func<MaintenanceHistory, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = m => (m.MaintenanceNumber.Contains(search)) ||
                    (m.Vehicle != null && m.Vehicle.LicensePlate.Contains(search)) ||
                    (m.Vehicle != null && m.Vehicle.User != null && m.Vehicle.User.FullName.Contains(search));
                sieveModel.Filters = RemoveFilter(sieveModel.Filters, "search");
            }
            if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                sieveModel.Sorts = $"-{nameof(MaintenanceHistory.MaintenanceDate)}";
            }
            var result = await repository.GetPagedAsync<MaintenanceTicketResponse>(
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
