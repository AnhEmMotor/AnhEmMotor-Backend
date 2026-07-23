using Application.ApiContracts.Admin.Analytics;
using Application.Interfaces.Repositories.Statistical;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Admin.Analytics;

public record GetAnalyticsChartsQuery : IRequest<AnalyticsChartsResponse>;

public class GetAnalyticsChartsHandler : IRequestHandler<GetAnalyticsChartsQuery, AnalyticsChartsResponse>
{
    private readonly IMemoryCache _cache;
    private readonly IStatisticalReadRepository _repo;

    public GetAnalyticsChartsHandler(IMemoryCache cache, IStatisticalReadRepository repo)
    {
        _cache = cache;
        _repo = repo;
    }

    public async Task<AnalyticsChartsResponse> Handle(GetAnalyticsChartsQuery req, CancellationToken ct)
    {
        var cacheKey = "analytics:charts";
        if (_cache.TryGetValue(cacheKey, out AnalyticsChartsResponse cached))
            return cached;

        var funnel = new List<CustomerFunnelDto>
        {
            new("Khách truy cập", 0),
            new("Khách đăng ký", 0),
            new("Khách tư vấn", 0),
            new("Lái thử", 0),
            new("Đặt cọc", 0),
            new("Hoàn tất", 0)
        };

        var structure = new List<ProductStructureDto>
        {
            new("Xe máy", 0),
            new("Phụ tùng", 0),
            new("Dịch vụ", 0)
        };

        var leaderboard = new List<SaleLeaderboardDto>();

        var result = new AnalyticsChartsResponse(funnel, structure, leaderboard);

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
        return result;
    }
}
