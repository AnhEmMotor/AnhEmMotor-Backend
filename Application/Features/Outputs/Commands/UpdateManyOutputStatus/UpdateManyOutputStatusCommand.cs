using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed record UpdateManyOutputStatusCommand : IRequest<ErrorResponse?>
{
    public ICollection<int> Ids { get; init; } = [];
    public string NewStatusId { get; init; } = string.Empty;
}
