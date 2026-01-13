
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed record DeleteSupplierCommand : IRequest<Result>
{
    public int Id { get; init; }
}