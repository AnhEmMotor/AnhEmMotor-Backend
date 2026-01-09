using Domain.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreBrand;

public sealed record RestoreBrandCommand : IRequest<(ApiContracts.Brand.Responses.BrandResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }
}
