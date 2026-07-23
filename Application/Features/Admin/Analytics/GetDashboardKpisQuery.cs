using Application.ApiContracts.Admin.Analytics;
using Application.Interfaces.Repositories.Statistical;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace Application.Features.Admin.Analytics;

public record GetDashboardKpisQuery(string Period = "month") : IRequest<DashboardKpisResponse>;

public class GetDashboardKpisHandler : IRequestHandler<GetDashboardKpisQuery, DashboardKpisResponse>
{
    private readonly IMemoryCache _cache;
    private readonly IStatisticalReadRepository _repo;

    public GetDashboardKpisHandler(IMemoryCache cache, IStatisticalReadRepository repo)
    {
        _cache = cache;
        _repo = repo;
    }

    public async Task<DashboardKpisResponse> Handle(GetDashboardKpisQuery req, CancellationToken ct)
    {
        var result = await _repo.GetDashboardKpisAsync(req.Period, ct);
        return result;
    }
}
