using Application.ApiContracts.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand;

public sealed record UpdateBrandCommand(int Id, string? Name, string? Description) : IRequest<(BrandResponse? Data, ErrorResponse? Error)>;
