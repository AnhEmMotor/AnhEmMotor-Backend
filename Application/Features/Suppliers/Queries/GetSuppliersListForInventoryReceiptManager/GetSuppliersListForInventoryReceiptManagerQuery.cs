using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;

namespace Application.Features.Suppliers.Queries.GetSuppliersListForInventoryReceiptManager;

public sealed record GetSuppliersListForInventoryReceiptManagerQuery : IRequest<Result<PagedResult<SupplierForInventoryReceiptManagerResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}

