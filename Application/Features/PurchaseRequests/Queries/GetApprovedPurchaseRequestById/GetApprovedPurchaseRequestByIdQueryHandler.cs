using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using Mapster;
using MediatR;
using System.Linq;

namespace Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequestById
{
    public sealed class GetApprovedPurchaseRequestByIdQueryHandler(IPurchaseRequestReadRepository repository) : IRequestHandler<GetApprovedPurchaseRequestByIdQuery, Result<ApprovedPurchaseRequestDetailResponse?>>
    {
        public async Task<Result<ApprovedPurchaseRequestDetailResponse?>> Handle(
            GetApprovedPurchaseRequestByIdQuery request,
            CancellationToken cancellationToken)
        {
            var pr = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id");
            }
            if (string.Compare(pr.Status, PurchaseRequestStatus.Approve) != 0)
            {
                return Error.Validation("Yêu cầu mua hàng chưa được phê duyệt.", "Status");
            }
            var response = pr.Adapt<ApprovedPurchaseRequestDetailResponse?>();
            if (response != null && request.ExcludeInventoryReceiptId.HasValue)
            {
                foreach (var responseItem in response.Items)
                {
                    var prItem = pr.PurchaseRequestItems.FirstOrDefault(x => x.Id == responseItem.Id);
                    if (prItem != null)
                    {
                        var excludedQty = prItem.InventoryReceiptInfos
                            .Where(
                                ii => ii.InventoryReceiptId == request.ExcludeInventoryReceiptId.Value &&
                                    ii.DeletedAt == null)
                            .Sum(ii => ii.Count ?? 0);
                        if (excludedQty > 0)
                        {
                            responseItem.UnimportedQuantity += excludedQty;
                        }
                    }
                }
            }
            return response;
        }
    }
}
