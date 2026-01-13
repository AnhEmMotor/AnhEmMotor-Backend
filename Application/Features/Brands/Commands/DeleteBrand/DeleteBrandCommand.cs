using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteBrand;

public sealed record DeleteBrandCommand : IRequest<Result>
{
    public int Id { get; init; }
}
