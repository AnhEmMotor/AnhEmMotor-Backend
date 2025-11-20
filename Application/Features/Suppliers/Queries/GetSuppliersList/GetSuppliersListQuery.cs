using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.GetSuppliersList;

public sealed record GetSuppliersListQuery(SieveModel SieveModel) : IRequest<PagedResult<SupplierResponse>>;
