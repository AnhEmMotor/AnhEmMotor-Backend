using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Services.Queries;

public class GetServicesListQuery : IRequest<Result<PagedResult<ServiceResponse>>>
{
    public SieveModel SieveModel { get; set; } = new();
}
