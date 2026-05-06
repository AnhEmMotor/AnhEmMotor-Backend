using Application.Common.Models;
using MediatR;

namespace Application.Features.Banners.Commands.DeleteBanner;

public sealed record DeleteBannerCommand(int Id) : IRequest<Result<Unit>>;
