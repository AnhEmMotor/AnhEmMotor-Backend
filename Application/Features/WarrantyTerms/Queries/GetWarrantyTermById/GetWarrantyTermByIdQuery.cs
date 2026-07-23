using Application.ApiContracts.WarrantyTerms.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermById;

public sealed record GetWarrantyTermByIdQuery(int Id) : IRequest<Result<WarrantyTermResponse>>;
