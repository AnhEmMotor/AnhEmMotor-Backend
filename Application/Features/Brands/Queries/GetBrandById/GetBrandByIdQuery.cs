
using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed record GetBrandByIdQuery(int Id) : IRequest<Result<BrandResponse?>>;
