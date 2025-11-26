using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed record UpdateManyOutputStatusCommand : IRequest<Unit>
{
    public ICollection<int> Ids { get; init; } = [];
    public string NewStatusId { get; init; } = string.Empty;
}
