using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using MediatR;

namespace Application.Features.Banners.Commands.TrackBannerClick;

public sealed class TrackBannerClickCommandHandler(
    IBannerReadRepository bannerReadRepository,
    IBannerUpdateRepository bannerUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<TrackBannerClickCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(TrackBannerClickCommand request, CancellationToken cancellationToken)
    {
        var banner = await bannerReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (banner == null) return Result<Unit>.Failure("Banner not found");

        banner.ClickCount++;
        
        bannerUpdateRepository.Update(banner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
