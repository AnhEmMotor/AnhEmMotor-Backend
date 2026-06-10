
namespace Application.ApiContracts.Banner.Responses;

public class BannerResponse
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string DesktopImageUrl { get; init; } = string.Empty;

    public string? MobileImageUrl { get; init; }

    public string? Description { get; init; }

    public string? CtaLink { get; init; }

    public string? CtaLabel { get; init; }

    public string? Placement { get; init; }

}
