using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.RestorePurchaseRequest;

public class RestorePurchaseRequestCommandHandler(
    IPurchaseRequestReadRepository readRepository,
    IPurchaseRequestUpdateRepository restoreRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestorePurchaseRequestCommand, Result<int>>
{
    public async Task<Result<int>> Handle(RestorePurchaseRequestCommand request, CancellationToken cancellationToken)
    {
        var purchaseRequest = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly).ConfigureAwait(false);

        if (purchaseRequest == null)
        {
            return Result<int>.Failure(Error.NotFound("Không tìm thấy yêu cầu mua hàng đã xóa."));
        }

        restoreRepository.Restore(purchaseRequest);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<int>.Success(purchaseRequest.Id);
    }
}
