using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Banners.Commands.CreateBanner;

public sealed record CreateBannerCommand : IRequest<Result<int>>
{
    public string Title { get; init; } = string.Empty;
    
    [JsonPropertyName("desktop_image_url")]
    public string DesktopImageUrl { get; init; } = string.Empty;

    [JsonPropertyName("mobile_image_url")]
    public string MobileImageUrl { get; init; } = string.Empty;
    
    [JsonPropertyName("link_url")]
    public string? LinkUrl { get; init; }

    [JsonPropertyName("cta_text")]
    public string? CtaText { get; init; }
    
    public string? Placement { get; init; }
    
    [JsonPropertyName("start_date")]
    public DateTimeOffset? StartDate { get; init; }
    
    [JsonPropertyName("end_date")]
    public DateTimeOffset? EndDate { get; init; }
    
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; } = true;
    
    public int Priority { get; init; }
}
