using Application.ApiContracts.Supplier.Responses;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed record GetDeletedSuppliersListQuery(SieveModel SieveModel) : IRequest<PagedResult<SupplierResponse>>;
