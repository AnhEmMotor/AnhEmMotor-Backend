using Application.ApiContracts.Input.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed record GetInputsBySupplierIdQuery(int SupplierId, SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<InputResponse>>;
