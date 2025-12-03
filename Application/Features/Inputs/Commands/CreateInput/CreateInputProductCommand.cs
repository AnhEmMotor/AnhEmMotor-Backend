namespace Application.Features.Inputs.Commands.CreateInput
{
    public sealed record CreateInputProductCommand
    {
        public int? ProductId { get; init; }

        public short? Count { get; init; }

        public long? InputPrice { get; init; }
    }
}
