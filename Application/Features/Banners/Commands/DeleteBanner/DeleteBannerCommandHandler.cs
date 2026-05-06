using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using MediatR;

namespace Application.Features.Banners.Commands.DeleteBanner;

public sealed class DeleteBannerCommandHandler(
    IBannerReadRepository bannerReadRepository,
    IBannerDeleteRepository bannerDeleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteBannerCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await bannerReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (banner == null) return Result<Unit>.Failure("Banner not found");

        bannerDeleteRepository.Delete(banner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
