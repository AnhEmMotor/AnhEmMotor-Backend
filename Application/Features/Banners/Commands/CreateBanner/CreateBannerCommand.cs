using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Banners.Commands.CreateBanner;

public sealed record CreateBannerCommand : IRequest<Result<int>>
{
    public string Title { get; init; } = string.Empty;
    
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; init; } = string.Empty;
    
    [JsonPropertyName("link_url")]
    public string? LinkUrl { get; init; }
    
    public string? Position { get; init; }
    
    [JsonPropertyName("start_date")]
    public DateTimeOffset? StartDate { get; init; }
    
    [JsonPropertyName("end_date")]
    public DateTimeOffset? EndDate { get; init; }
    
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; } = true;
    
    [JsonPropertyName("display_order")]
    public int DisplayOrder { get; init; }
}
