using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Banners.Commands.CreateBanner;

public sealed class CreateBannerCommandHandler(
    IBannerInsertRepository bannerInsertRepository,
    IBannerAuditRepository bannerAuditRepository,
    IHttpTokenAccessorService tokenAccessorService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBannerCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = new Domain.Entities.Banner
        {
            Title = request.Title.Trim(),
            DesktopImageUrl = request.DesktopImageUrl.Trim(),
            MobileImageUrl = request.MobileImageUrl.Trim(),
            LinkUrl = request.LinkUrl?.Trim(),
            CtaText = request.CtaText?.Trim(),
            Placement = request.Placement?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            Priority = request.Priority
        };

        bannerInsertRepository.Add(banner);

        // Audit Log
        bannerAuditRepository.AddLog(new Domain.Entities.BannerAuditLog
        {
            Banner = banner,
            Action = "Create",
            ChangedBy = tokenAccessorService.GetUserId() ?? "Unknown",
            Details = $"Created banner '{banner.Title}'"
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(banner.Id);
    }
}
