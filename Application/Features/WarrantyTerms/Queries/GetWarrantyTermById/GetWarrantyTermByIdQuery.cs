using Application.ApiContracts.Admin.Warranty;
using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermById;

public sealed record GetWarrantyTermByIdQuery : IRequest<Result<WarrantyTermResponse?>>
{
    public int Id { get; init; }
}
