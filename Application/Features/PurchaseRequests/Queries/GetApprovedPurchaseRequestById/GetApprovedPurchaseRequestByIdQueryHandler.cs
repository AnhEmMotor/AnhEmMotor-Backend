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
            return pr.Adapt<ApprovedPurchaseRequestDetailResponse?>();
        }
    }
}
