using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using Domain.Entities;
using MediatR;

namespace Application.Features.Banners.Commands.CreateBanner;

public sealed class CreateBannerCommandHandler(IBannerInsertRepository bannerInsertRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateBannerCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = new Banner
        {
            Title = request.Title.Trim(),
            ImageUrl = request.ImageUrl.Trim(),
            LinkUrl = request.LinkUrl?.Trim(),
            Position = request.Position?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };
        bannerInsertRepository.Add(banner);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(banner.Id);
    }
}
