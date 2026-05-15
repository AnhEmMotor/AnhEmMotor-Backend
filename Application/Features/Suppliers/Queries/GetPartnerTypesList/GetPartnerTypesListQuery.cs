using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetPartnerTypesList;

public sealed record GetPartnerTypesListQuery : IRequest<Result<List<PartnerTypeResponse>>>;

public sealed record PartnerTypeResponse(string Key, string Name);
