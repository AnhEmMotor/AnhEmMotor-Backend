using Application.ApiContracts.Input.Responses;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInputStatus;

public sealed record UpdateInputStatusCommand : IRequest<(InputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string StatusId { get; init; } = string.Empty;
}
