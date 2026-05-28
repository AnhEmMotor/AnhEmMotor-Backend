using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById
{
    public sealed class GetPurchaseRequestByIdQueryHandler(IPurchaseRequestReadRepository repository)
        : IRequestHandler<GetPurchaseRequestByIdQuery, Result<PurchaseRequestDetailResponse?>>
    {
        public async Task<Result<PurchaseRequestDetailResponse?>> Handle(
            GetPurchaseRequestByIdQuery request,
            CancellationToken cancellationToken)
        {
            var pr = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id");
            }
            return pr.Adapt<PurchaseRequestDetailResponse?>();
        }
    }
}
