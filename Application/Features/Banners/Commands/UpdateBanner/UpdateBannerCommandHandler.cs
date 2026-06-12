using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Banners.Commands.UpdateBanner;

public sealed class UpdateBannerCommandHandler(
    IBannerReadRepository bannerReadRepository,
    IBannerInsertRepository bannerInsertRepository,
    IBannerUpdateRepository bannerUpdateRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBannerCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await bannerReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (banner == null)
            return Result<Unit>.Failure("Banner not found");
        banner.Title = request.Title.Trim();
        banner.DesktopImageUrl = request.DesktopImageUrl.Trim();
        banner.MobileImageUrl = request.MobileImageUrl?.Trim();
        banner.Description = request.Description?.Trim();
        banner.CtaLink = request.CtaLink?.Trim();
        banner.CtaLabel = request.CtaLabel?.Trim();
        banner.Placement = request.Placement?.Trim();
        bannerUpdateRepository.Update(banner);
        var action = "Update";
        bannerInsertRepository.AddLog(
            new BannerAuditLog
            {
                Banner = banner,
                Action = action,
                ChangedBy = currentUserContext.GetUserId().ToString(),
                Details = $"Updated banner '{banner.Title}'"
            });
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Unit>.Success(Unit.Value);
    }
}
