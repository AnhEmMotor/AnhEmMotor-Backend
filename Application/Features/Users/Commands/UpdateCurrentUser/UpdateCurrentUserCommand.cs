using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand : IRequest<Result<UserDTOForManagerResponse>>
{
    [JsonIgnore]
    public string? UserId { get; init; }

    public string? FullName { get; init; }

    public string? Gender { get; init; }

    public string? PhoneNumber { get; init; }
}
