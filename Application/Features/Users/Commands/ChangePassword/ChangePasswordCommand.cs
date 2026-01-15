using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;
using Application;
using Application.Features;
using Application.Features.Users;
using Application.Features.Users.Commands;

namespace Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result<ChangePasswordByUserResponse>>
{
    public string? UserId { get; init; }

    public string? CurrentPassword { get; init; }

    public string? NewPassword { get; init; }
}
