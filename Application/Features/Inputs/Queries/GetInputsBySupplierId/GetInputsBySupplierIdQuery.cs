using Application.ApiContracts.Input.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed record GetInputsBySupplierIdQuery(int SupplierId, SieveModel SieveModel) : IRequest<Result<PagedResult<InputResponse>>>;
