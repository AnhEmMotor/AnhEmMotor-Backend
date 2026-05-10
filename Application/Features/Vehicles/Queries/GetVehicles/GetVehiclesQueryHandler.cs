using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Vehicles.Queries.GetVehicles
{
    public sealed class GetVehiclesQueryHandler(
        IVehicleReadRepository vehicleRepository,
        ISievePaginator sievePaginator) 
        : IRequestHandler<GetVehiclesQuery, Result<PagedResult<VehicleResponse>>>
    {
        public async Task<Result<PagedResult<VehicleResponse>>> Handle(
            GetVehiclesQuery request,
            CancellationToken cancellationToken)
        {
            var query = vehicleRepository.GetQuery(DataFetchMode.ActiveOnly);

            var sieveModel = request.SieveModel ?? new SieveModel();
            
            // Trích xuất logic search từ Filters nếu có (ví dụ: ?Filters=search@=abc)
            var search = ExtractFilterValue(sieveModel.Filters, "search");
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(v => 
                    v.LicensePlate.Contains(search) || 
                    v.VinNumber.Contains(search) || 
                    v.Lead.FullName.Contains(search));
                
                // Loại bỏ search khỏi filters để Sieve không báo lỗi thuộc tính không tồn tại
                sieveModel.Filters = RemoveFilter(sieveModel.Filters, "search");
            }

            if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                sieveModel.Sorts = $"-{nameof(Domain.Entities.Vehicle.PurchaseDate)}";
            }

            var result = await sievePaginator.ApplyAsync<Domain.Entities.Vehicle, VehicleResponse>(
                query,
                sieveModel,
                DataFetchMode.ActiveOnly,
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
            parts.RemoveAll(p => p.Split(['=', '@', '!', '<', '>'], 2)[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase));
            return string.Join(",", parts);
        }
    }
}
