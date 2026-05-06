using Application.Common.Models;
using MediatR;

namespace Application.Features.Banners.Commands.TrackBannerClick;

public sealed record TrackBannerClickCommand(int Id) : IRequest<Result<Unit>>;
