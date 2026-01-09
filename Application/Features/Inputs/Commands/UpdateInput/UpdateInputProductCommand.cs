namespace Application.Features.Inputs.Commands.UpdateInput;

public sealed record UpdateInputProductCommand
{
    public int? Id { get; init; }

    public int? ProductId { get; init; }

    public int? Count { get; init; }

    public decimal? InputPrice { get; init; }
}
