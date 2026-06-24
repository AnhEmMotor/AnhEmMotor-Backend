using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.CloneManySuppliers;

public sealed record CloneManySuppliersCommand : IRequest<Result>
{
    public List<int> Ids { get; init; } = [];
}
