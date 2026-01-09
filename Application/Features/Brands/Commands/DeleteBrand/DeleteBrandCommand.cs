using Domain.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteBrand;

public sealed record DeleteBrandCommand : IRequest<Common.Models.ErrorResponse?>
{
    public int Id { get; init; }
}
