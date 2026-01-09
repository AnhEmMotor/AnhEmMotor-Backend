using Application.ApiContracts.Supplier.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierById;

public sealed record GetSupplierByIdQuery(int Id) : IRequest<(SupplierResponse? Data, Common.Models.ErrorResponse? Error)>;
