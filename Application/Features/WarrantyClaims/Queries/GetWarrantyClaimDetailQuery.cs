using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyClaimDetailQuery : IRequest<Result<WarrantyClaimDetailResponse?>>
{
    public int Id { get; set; }
}
