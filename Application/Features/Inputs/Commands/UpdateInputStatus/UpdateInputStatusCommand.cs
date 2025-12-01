using Application.ApiContracts.Input;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInputStatus;

public sealed record UpdateInputStatusCommand : IRequest<(InputResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }
    public string StatusId { get; init; } = string.Empty;
}
