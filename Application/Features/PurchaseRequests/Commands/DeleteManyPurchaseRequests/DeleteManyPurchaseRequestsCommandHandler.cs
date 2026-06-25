using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.DeleteManyPurchaseRequests;

public class DeleteManyPurchaseRequestsCommandHandler(
    IPurchaseRequestReadRepository readRepository,
    IPurchaseRequestDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyPurchaseRequestsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(DeleteManyPurchaseRequestsCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return Result<int>.Failure(Error.BadRequest("Danh sách ID trống."));
        }

        var sourceRequests = new List<Domain.Entities.PurchaseRequest>();
        foreach (var id in request.Ids)
        {
            var req = await readRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (req != null)
            {
                sourceRequests.Add(req);
            }
        }

        if (sourceRequests == null || sourceRequests.Count == 0)
        {
            return Result<int>.Failure(Error.NotFound("Không tìm thấy yêu cầu mua hàng."));
        }

        int deletedCount = 0;

        foreach (var src in sourceRequests)
        {
            deleteRepository.Delete(src);
            deletedCount++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<int>.Success(deletedCount);
    }
}
