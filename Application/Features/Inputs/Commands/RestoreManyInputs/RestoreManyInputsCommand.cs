using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed record RestoreManyInputsCommand : IRequest<Unit>
{
    public ICollection<int> Ids { get; init; } = [];
}
