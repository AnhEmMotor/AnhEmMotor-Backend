using Application.ApiContracts.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreManyBrands;

public sealed record RestoreManyBrandsCommand(List<int> Ids) : IRequest<(List<BrandResponse>? Data, ErrorResponse? Error)>;
