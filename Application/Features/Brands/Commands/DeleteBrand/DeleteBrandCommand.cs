using MediatR;
using Application.Common.Models;

namespace Application.Features.Brands.Commands.DeleteBrand;

public sealed record DeleteBrandCommand : IRequest<Result>
{
    public int Id { get; init; }
}
