using Application.ApiContracts.User.Requests;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result<ChangePasswordByManagerResponse>>
{
    public Guid? UserId { get; init; }
    public string? CurrentPassword { get; init; }
    public string? NewPassword { get; init; }
    public string? CurrentUserId { get; init; }
}