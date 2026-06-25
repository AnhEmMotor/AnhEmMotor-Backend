using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.RestoreManyPurchaseRequests;

public class RestoreManyPurchaseRequestsCommandHandler(
    IPurchaseRequestReadRepository readRepository,
    IPurchaseRequestUpdateRepository restoreRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyPurchaseRequestsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(RestoreManyPurchaseRequestsCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return Result<int>.Failure(Error.BadRequest("Danh sách ID trống."));
        }

        var sourceRequests = new List<Domain.Entities.PurchaseRequest>();
        foreach (var id in request.Ids)
        {
            var req = await readRepository.GetByIdAsync(id, cancellationToken, DataFetchMode.DeletedOnly).ConfigureAwait(false);
            if (req != null)
            {
                sourceRequests.Add(req);
            }
        }

        if (sourceRequests == null || sourceRequests.Count == 0)
        {
            return Result<int>.Failure(Error.NotFound("Không tìm thấy yêu cầu mua hàng đã xóa."));
        }

        int restoredCount = 0;

        foreach (var src in sourceRequests)
        {
            restoreRepository.Restore(src);
            restoredCount++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<int>.Success(restoredCount);
    }
}
