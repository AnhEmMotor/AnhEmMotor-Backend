using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Banner;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Banners.Queries.GetBannersList
{
    public class GetBannersListQueryHandler(IBannerReadRepository bannerRepository) : IRequestHandler<GetBannersListQuery, Result<List<BannerResponse>>>
    {
        public async Task<Result<List<BannerResponse>>> Handle(
            GetBannersListQuery request,
            CancellationToken cancellationToken)
        {
            var banners = await bannerRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var response = banners.Select(
                b => new BannerResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    ImageUrl = b.ImageUrl,
                    LinkUrl = b.LinkUrl,
                    CtaText = b.CtaText,
                    Placement = b.Placement,
                    Position = b.Position,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    IsActive = b.IsActive,
                    Priority = b.Priority,
                    ClickCount = b.ClickCount,
                    ViewCount = b.ViewCount,
                    DisplayOrder = b.DisplayOrder,
                    CTR = b.CTR
                })
                .OrderBy(b => b.DisplayOrder)
                .ToList();
            return Result<List<BannerResponse>>.Success(response);
        }
    }

}
