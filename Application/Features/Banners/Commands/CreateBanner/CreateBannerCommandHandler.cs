using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Banners.Commands.CreateBanner;

public class CreateBannerCommandHandler(
    IBannerInsertRepository bannerInsertRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBannerCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = new Banner
        {
            Title = request.Title.Trim(),
            DesktopImageUrl = request.DesktopImageUrl.Trim(),
            MobileImageUrl = request.MobileImageUrl?.Trim(),
            Description = request.Description?.Trim(),
            CtaLink = request.CtaLink?.Trim(),
            CtaLabel = request.CtaLabel?.Trim(),
            Placement = request.Placement?.Trim()
        };
        bannerInsertRepository.Add(banner);
        bannerInsertRepository.AddLog(
            new BannerAuditLog
            {
                Banner = banner,
                Action = "Create",
                ChangedBy = currentUserContext.GetUserId().ToString(),
                Details = $"Created banner '{banner.Title}'"
            });
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(banner.Id);
    }
}
