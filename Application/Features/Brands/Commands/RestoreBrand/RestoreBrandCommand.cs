using Application.ApiContracts.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreBrand;

public sealed record RestoreBrandCommand(int Id) : IRequest<(BrandResponse? Data, ErrorResponse? Error)>;
