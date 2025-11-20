using Application.ApiContracts.Brand;
using MediatR;

namespace Application.Features.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(string? Name, string? Description) : IRequest<BrandResponse>;
