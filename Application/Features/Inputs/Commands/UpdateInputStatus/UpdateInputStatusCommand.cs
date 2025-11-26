using Application.ApiContracts.Input;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInputStatus;

public sealed record UpdateInputStatusCommand : IRequest<InputResponse>
{
    public int Id { get; init; }
    public string StatusId { get; init; } = string.Empty;
}
