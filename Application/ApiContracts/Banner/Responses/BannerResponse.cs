
namespace Application.ApiContracts.Banner.Responses;

public class BannerResponse
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public string? LinkUrl { get; init; }

    public string? CtaText { get; init; }

    public string? Placement { get; init; }
    public string? Position { get; init; }

    public DateTimeOffset? StartDate { get; init; }

    public DateTimeOffset? EndDate { get; init; }

    public bool IsActive { get; init; }

    public int Priority { get; init; }

    public int ClickCount { get; init; }

    public int ViewCount { get; init; }

    public double CTR { get; init; }
    public int DisplayOrder { get; init; }
}
