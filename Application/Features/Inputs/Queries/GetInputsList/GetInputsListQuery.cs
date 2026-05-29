using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.InventoryReceipts.Queries.GetInputsList;

public sealed record GetInputsListQuery : IRequest<Result<PagedResult<InputListResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
