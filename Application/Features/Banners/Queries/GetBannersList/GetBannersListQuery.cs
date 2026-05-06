using Application.Common.Models;
using Application.Interfaces.Repositories.Banner;
using MediatR;

namespace Application.Features.Banners.Queries.GetBannersList;

public sealed class GetBannersListQuery : IRequest<Result<List<BannerResponse>>>
{
}

public class BannerResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string DesktopImageUrl { get; set; } = string.Empty;

    public string MobileImageUrl { get; set; } = string.Empty;

    public string? LinkUrl { get; set; }

    public string? CtaText { get; set; }

    public string? Placement { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public bool IsActive { get; set; }

    public int Priority { get; set; }

    public int ClickCount { get; set; }

    public int ViewCount { get; set; }

    public double CTR { get; set; }
}

public class GetBannersListQueryHandler(IBannerReadRepository bannerRepository) : IRequestHandler<GetBannersListQuery, Result<List<BannerResponse>>>
{
    public async Task<Result<List<BannerResponse>>> Handle(
        GetBannersListQuery request,
        CancellationToken cancellationToken)
    {
        var banners = await bannerRepository.GetAllAsync(cancellationToken);
        var response = banners.Select(
            b => new BannerResponse
            {
                Id = b.Id,
                Title = b.Title,
                DesktopImageUrl = b.DesktopImageUrl,
                MobileImageUrl = b.MobileImageUrl,
                LinkUrl = b.LinkUrl,
                CtaText = b.CtaText,
                Placement = b.Placement,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                IsActive = b.IsActive,
                Priority = b.Priority,
                ClickCount = b.ClickCount,
                ViewCount = b.ViewCount,
                CTR = b.CTR
            })
            .ToList();
        return Result<List<BannerResponse>>.Success(response);
    }
}
