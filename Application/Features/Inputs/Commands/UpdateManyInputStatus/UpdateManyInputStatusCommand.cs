using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateManyInputStatus;

public sealed record UpdateManyInputStatusCommand : IRequest<ErrorResponse?>
{
    public ICollection<int> Ids { get; init; } = [];
    public string StatusId { get; init; } = string.Empty;
}
