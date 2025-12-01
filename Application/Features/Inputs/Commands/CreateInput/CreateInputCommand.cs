using Application.ApiContracts.Input;
using MediatR;

namespace Application.Features.Inputs.Commands.CreateInput;

public sealed record CreateInputCommand : IRequest<InputResponse>
{
    public string? Notes { get; init; }

    public string? StatusId { get; init; }

    public int? SupplierId { get; init; }

    public List<CreateInputProductCommand> Products { get; init; } = [];
}

public sealed record CreateInputProductCommand
{
    public int? ProductId { get; init; }

    public short? Count { get; init; }

    public long? InputPrice { get; init; }
}
