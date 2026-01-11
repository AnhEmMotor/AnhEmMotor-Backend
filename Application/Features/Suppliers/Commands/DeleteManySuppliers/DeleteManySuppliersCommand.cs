
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed record DeleteManySuppliersCommand : IRequest<Result>
{
    public List<int> Ids { get; init; } = [];
}