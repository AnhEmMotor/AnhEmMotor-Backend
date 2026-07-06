using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyClaim;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyClaimDetailQuery : IRequest<Result<WarrantyClaimResponse?>>
{
    public int Id { get; set; }
}
