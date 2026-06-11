using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Banner;
using Mapster;
using MediatR;

namespace Application.Features.Banners.Queries.GetStoreBanners;

public class GetStoreBannersQueryHandler(IBannerReadRepository bannerReadRepository) : IRequestHandler<GetStoreBannersQuery, Result<List<BannerResponse>>>
{
    public async Task<Result<List<BannerResponse>>> Handle(
        GetStoreBannersQuery request,
        CancellationToken cancellationToken)
    {
        var banners = await bannerReadRepository.GetBannersByPlacementAsync(cancellationToken, request.Placement)
            .ConfigureAwait(false);
        var response = banners.Adapt<List<BannerResponse>>();
        return Result<List<BannerResponse>>.Success(response);
    }
}
