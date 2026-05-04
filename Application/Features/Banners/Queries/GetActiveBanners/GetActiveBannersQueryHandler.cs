using Application.Common.Models;
using Application.Interfaces.Repositories.Banner;
using Mapster;
using MediatR;

namespace Application.Features.Banners.Queries.GetActiveBanners;

public sealed class GetActiveBannersQueryHandler(IBannerReadRepository bannerReadRepository) : IRequestHandler<GetActiveBannersQuery, Result<List<BannerResponse>>>
{
    public async Task<Result<List<BannerResponse>>> Handle(
        GetActiveBannersQuery request,
        CancellationToken cancellationToken)
    {
        var banners = await bannerReadRepository.GetActiveBannersAsync(cancellationToken);
        return Result<List<BannerResponse>>.Success(banners.Adapt<List<BannerResponse>>());
    }
}
