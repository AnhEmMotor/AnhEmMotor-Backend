using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Entities;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.CloneManyPurchaseRequests;

public class CloneManyPurchaseRequestsCommandHandler(
    IPurchaseRequestReadRepository readRepository,
    IPurchaseRequestInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CloneManyPurchaseRequestsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CloneManyPurchaseRequestsCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return Result<int>.Failure(Error.BadRequest("Danh sách ID trống."));
        }
        var sourceRequests = new List<PurchaseRequest>();
        foreach (var id in request.Ids)
        {
            var req = await readRepository.GetByIdWithDetailsAsync(id, cancellationToken).ConfigureAwait(false);
            if (req != null)
            {
                sourceRequests.Add(req);
            }
        }
        if (sourceRequests == null || sourceRequests.Count == 0)
        {
            return Result<int>.Failure(Error.NotFound("Không tìm thấy yêu cầu mua hàng."));
        }
        int clonedCount = 0;
        foreach (var src in sourceRequests)
        {
            var newRequest = new PurchaseRequest
            {
                Status = "draft",
                Note = src.Note + " (Bản sao)",
                CreatedBy = null,
                SentBy = null,
                ApprovedBy = null,
                RejectedBy = null,
                PurchaseRequestItems =
                    src.PurchaseRequestItems?.Select(
                            item => new PurchaseRequestItem
                            {
                                ProductVariantId = item.ProductVariantId,
                                ProductVariantColorId = item.ProductVariantColorId,
                                Quantity = item.Quantity,
                                SupplierId = item.SupplierId,
                                UnitPrice = item.UnitPrice,
                                ProductQuotationId = item.ProductQuotationId
                            })
                            .ToList() ??
                        new List<PurchaseRequestItem>()
            };
            insertRepository.Add(newRequest);
            clonedCount++;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(clonedCount);
    }
}
