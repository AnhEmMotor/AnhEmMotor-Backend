using Application.ApiContracts.WarrantyClaim.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries.GetWarrantyClaimDetail
{
    public class GetWarrantyClaimDetailQuery : IRequest<Result<WarrantyClaimDetailResponse>>
    {
        public int Id { get; set; }
    }
}
