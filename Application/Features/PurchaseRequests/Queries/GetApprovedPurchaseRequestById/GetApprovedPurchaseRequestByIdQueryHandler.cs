using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequestById
{
    public sealed class GetApprovedPurchaseRequestByIdQueryHandler(IPurchaseRequestReadRepository repository)
        : IRequestHandler<GetApprovedPurchaseRequestByIdQuery, Result<ApprovedPurchaseRequestDetailResponse?>>
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
            if (pr.Status != PurchaseRequestStatus.Approve)
            {
                return Error.Validation("Yêu cầu mua hàng chưa được phê duyệt.", "Status");
            }

            var response = pr.Adapt<ApprovedPurchaseRequestDetailResponse?>();

            // Adjust remaining quantities if a PO is to be excluded (e.g., when editing)
            if (response != null && request.ExcludePurchaseOrderId.HasValue)
            {
                foreach (var responseItem in response.Items)
                {
                    var prItem = pr.PurchaseRequestItems.FirstOrDefault(x => x.Id == responseItem.Id);
                    if (prItem != null)
                    {
                        var excludedQty = prItem.PurchaseOrderItems
                            .Where(poi => poi.PurchaseOrderId == request.ExcludePurchaseOrderId.Value && poi.DeletedAt == null)
                            .Sum(poi => poi.OrderedQuantity);
                        
                        if (excludedQty > 0)
                        {
                            responseItem.PORemainingQuantity += excludedQty;
                        }
                    }
                }
            }

            return response;
        }
    }
}
