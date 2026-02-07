using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result<ChangePasswordByUserResponse>>
{
    [JsonIgnore]
    public string? UserId { get; init; }

    public string? CurrentPassword { get; init; }

    public string? NewPassword { get; init; }
}
