using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Banners.Commands.UpdateBanner;

public sealed class UpdateBannerCommandHandler(
    IBannerReadRepository bannerReadRepository,
    IBannerUpdateRepository bannerUpdateRepository,
    IBannerAuditRepository bannerAuditRepository,
    IHttpTokenAccessorService tokenAccessorService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBannerCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await bannerReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (banner == null) return Result<Unit>.Failure("Banner not found");

        var oldStatus = banner.IsActive;
        
        banner.Title = request.Title.Trim();
        banner.DesktopImageUrl = request.DesktopImageUrl.Trim();
        banner.MobileImageUrl = request.MobileImageUrl.Trim();
        banner.LinkUrl = request.LinkUrl?.Trim();
        banner.CtaText = request.CtaText?.Trim();
        banner.Placement = request.Placement?.Trim();
        banner.StartDate = request.StartDate;
        banner.EndDate = request.EndDate;
        banner.IsActive = request.IsActive;
        banner.Priority = request.Priority;

        bannerUpdateRepository.Update(banner);

        // Audit Log
        var action = oldStatus != banner.IsActive ? (banner.IsActive ? "Resume" : "Pause") : "Update";
        bannerAuditRepository.AddLog(new Domain.Entities.BannerAuditLog
        {
            Banner = banner,
            Action = action,
            ChangedBy = tokenAccessorService.GetUserId() ?? "Unknown",
            Details = $"Updated banner '{banner.Title}'"
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
