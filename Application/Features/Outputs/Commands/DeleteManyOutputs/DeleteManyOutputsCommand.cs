using MediatR;

namespace Application.Features.Outputs.Commands.DeleteManyOutputs;

public sealed record DeleteManyOutputsCommand : IRequest<Unit>
{
    public ICollection<int> Ids { get; init; } = [];
}
