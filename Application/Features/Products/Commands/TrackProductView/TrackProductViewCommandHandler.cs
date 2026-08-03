using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services;
using MediatR;
using ProductViewEntity = Domain.Entities.ProductView;

namespace Application.Features.Products.Commands.TrackProductView;

public class TrackProductViewCommandHandler(
    IProductViewRepository productViewRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<TrackProductViewCommand, Result>
{
    public async Task<Result> Handle(TrackProductViewCommand request, CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0)
        {
            return Result.Failure(Error.Validation("ProductId không hợp lệ."));
        }

        productViewRepository.Add(
            new ProductViewEntity
            {
                ProductId = request.ProductId,
                CustomerUserId = currentUserContext.GetUserIdOrNull(),
                VisitorKey = string.IsNullOrWhiteSpace(request.VisitorKey) ? null : request.VisitorKey,
                DwellTimeMs = Math.Max(0, request.DwellTimeMs)
            });
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
