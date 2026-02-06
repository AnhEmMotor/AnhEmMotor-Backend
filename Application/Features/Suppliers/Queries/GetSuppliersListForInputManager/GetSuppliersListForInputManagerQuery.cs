using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;

namespace Application.Features.Suppliers.Queries.GetSuppliersListForInputManager;

public sealed record GetSuppliersListForInputManagerQuery : IRequest<Result<PagedResult<SupplierForInputManagerResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}

