using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Banners.Commands.UpdateBanner;

public sealed record UpdateBannerCommand : IRequest<Result<Unit>>
{
    [JsonIgnore]
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("desktop_image_url")]
    public string DesktopImageUrl { get; init; } = string.Empty;

    [JsonPropertyName("mobile_image_url")]
    public string? MobileImageUrl { get; init; }

    public string? Description { get; init; }

    [JsonPropertyName("cta_link")]
    public string? CtaLink { get; init; }

    [JsonPropertyName("cta_label")]
    public string? CtaLabel { get; init; }

    public string? Placement { get; init; }

}
