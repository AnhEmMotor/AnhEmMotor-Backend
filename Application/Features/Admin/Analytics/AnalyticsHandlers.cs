using MediatR;
using AnhEmMotor.Application.ApiContracts.Admin.Analytics;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AnhEmMotor.Application.Features.Admin.Analytics
{
    public record GetDashboardKpisQuery() : IRequest<DashboardKpisResponse>;
    public record GetAnalyticsChartsQuery() : IRequest<AnalyticsChartsResponse>;

    public class GetDashboardKpisHandler : IRequestHandler<GetDashboardKpisQuery, DashboardKpisResponse>
    {
        private readonly IMemoryCache _cache;
        public GetDashboardKpisHandler(IMemoryCache cache) => _cache = cache;

        public async Task<DashboardKpisResponse> Handle(GetDashboardKpisQuery request, CancellationToken cancellationToken)
        {
            var fallback = new DashboardKpisResponse(500000000, 120, 15, 75.5, 120000000, 85.0);

            return await _cache.GetOrCreateAsync("admin_dashboard_kpis", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return Task.FromResult(fallback);
            }) ?? fallback;
        }
    }

    public class GetAnalyticsChartsHandler : IRequestHandler<GetAnalyticsChartsQuery, AnalyticsChartsResponse>
    {
        private readonly IMemoryCache _cache;
        public GetAnalyticsChartsHandler(IMemoryCache cache) => _cache = cache;

        public async Task<AnalyticsChartsResponse> Handle(GetAnalyticsChartsQuery request, CancellationToken cancellationToken)
        {
            var fallback = new AnalyticsChartsResponse(
                new List<CustomerFunnelDto> { new("Showroom", 1000), new("TestDrive", 300), new("Closed", 100) },
                new List<ProductStructureDto> { new("Electric", 20), new("Petrol", 70), new("Luxury", 10) },
                new List<SaleLeaderboardDto> { new("Sale A", 1000000000), new("Sale B", 800000000), new("Sale C", 600000000) }
            );

            return await _cache.GetOrCreateAsync("admin_analytics_charts", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return Task.FromResult(fallback);
            }) ?? fallback;
        }
    }
}
