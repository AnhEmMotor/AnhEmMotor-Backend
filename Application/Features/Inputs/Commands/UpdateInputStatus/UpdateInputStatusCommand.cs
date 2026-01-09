using Application.ApiContracts.Input.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInputStatus;

public sealed record UpdateInputStatusCommand : IRequest<(InputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string StatusId { get; init; } = string.Empty;

    public Guid? CurrentUserId { get; init; }
}

