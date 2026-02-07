using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Inputs.Commands.UpdateInputStatus;

public sealed record UpdateInputStatusCommand : IRequest<Result<InputResponse>>
{
    public int Id { get; init; }

    public string StatusId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid? CurrentUserId { get; init; }
}

