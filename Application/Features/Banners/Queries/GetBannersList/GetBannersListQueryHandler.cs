using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Banner;
using Mapster;
using MediatR;
using System;

using Domain.Primitives;
using System.Linq;

namespace Application.Features.Banners.Queries.GetBannersList
{
    public class GetBannersListQueryHandler(IBannerReadRepository bannerRepository) : IRequestHandler<GetBannersListQuery, Result<PagedResult<BannerResponse>>>
    {
        public async Task<Result<PagedResult<BannerResponse>>> Handle(
            GetBannersListQuery request,
            CancellationToken cancellationToken)
        {
            var banners = await bannerRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            
            var totalCount = banners.Count;
            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            
            var pagedBanners = banners
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = pagedBanners.Adapt<List<BannerResponse>>();
            
            var pagedResult = new PagedResult<BannerResponse>(response, totalCount, page, pageSize);
            return Result<PagedResult<BannerResponse>>.Success(pagedResult);
        }
    }
}
