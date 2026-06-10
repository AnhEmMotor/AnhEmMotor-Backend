using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Banners.Commands.CreateBanner;

public sealed record CreateBannerCommand : IRequest<Result<int>>
{
    public string Title { get; init; } = string.Empty;

    public string DesktopImageUrl { get; init; } = string.Empty;

    public string? MobileImageUrl { get; init; }

    public string? Description { get; init; }

    public string? CtaLink { get; init; }

    public string? CtaLabel { get; init; }

    public string? Placement { get; init; }

}
