using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed record DeleteManyInputsCommand : IRequest<Unit>
{
    public ICollection<int> Ids { get; init; } = [];
}
