using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed record GetDeletedSuppliersListQuery(SieveModel SieveModel) : IRequest<PagedResult<SupplierResponse>>;
