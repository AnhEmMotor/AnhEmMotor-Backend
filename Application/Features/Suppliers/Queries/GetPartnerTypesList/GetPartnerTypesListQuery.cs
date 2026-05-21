using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetPartnerTypesList;

public sealed record GetPartnerTypesListQuery : IRequest<Result<List<PartnerTypeResponse>>>;

