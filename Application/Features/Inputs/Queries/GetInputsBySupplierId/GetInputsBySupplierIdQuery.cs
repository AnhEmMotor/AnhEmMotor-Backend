using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed record GetInputsBySupplierIdQuery : IRequest<Result<PagedResult<InputResponse>>>
{
    public int SupplierId { get; init; }

    public SieveModel? SieveModel { get; init; }
}
