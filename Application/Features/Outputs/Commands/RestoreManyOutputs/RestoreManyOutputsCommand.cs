using MediatR;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed record RestoreManyOutputsCommand : IRequest<Unit>
{
    public ICollection<int> Ids { get; init; } = [];
}
