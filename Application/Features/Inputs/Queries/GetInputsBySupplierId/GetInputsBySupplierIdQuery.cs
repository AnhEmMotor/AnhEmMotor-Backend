using Application.ApiContracts.Input;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed record GetInputsBySupplierIdQuery(int SupplierId, SieveModel SieveModel) : IRequest<PagedResult<InputResponse>>;
