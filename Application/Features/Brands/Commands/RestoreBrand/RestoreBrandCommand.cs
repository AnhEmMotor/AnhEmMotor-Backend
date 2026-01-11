using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreBrand;

public sealed record RestoreBrandCommand : IRequest<Result<BrandResponse>>
{
    public int Id { get; init; }
}
