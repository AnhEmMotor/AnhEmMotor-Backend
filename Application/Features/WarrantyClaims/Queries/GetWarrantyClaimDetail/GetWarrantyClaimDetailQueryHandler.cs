using Application.ApiContracts.WarrantyClaim.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyClaim;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WarrantyClaims.Queries.GetWarrantyClaimDetail
{
    public class GetWarrantyClaimDetailQueryHandler(IWarrantyClaimReadRepository warrantyClaimReadRepository)
        : IRequestHandler<GetWarrantyClaimDetailQuery, Result<WarrantyClaimDetailResponse>>
    {
        public async Task<Result<WarrantyClaimDetailResponse>> Handle(
            GetWarrantyClaimDetailQuery request,
            CancellationToken cancellationToken)
        {
            var claim = await warrantyClaimReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (claim == null)
            {
                return Result<WarrantyClaimDetailResponse>.Failure(Error.NotFound("Không tìm thấy hồ sơ bảo hành."));
            }
            var response = claim.Adapt<WarrantyClaimDetailResponse>();
            return Result<WarrantyClaimDetailResponse>.Success(response);
        }
    }
}
