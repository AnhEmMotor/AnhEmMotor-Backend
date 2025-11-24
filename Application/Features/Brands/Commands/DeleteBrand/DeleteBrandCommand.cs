using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteBrand;

public sealed record DeleteBrandCommand : IRequest<ErrorResponse?>
{
    public int Id { get; init; }
}
