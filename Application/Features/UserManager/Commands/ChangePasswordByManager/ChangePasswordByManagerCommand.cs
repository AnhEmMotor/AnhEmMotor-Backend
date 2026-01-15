using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;
using Application;
using Application.Features;
using Application.Features.UserManager;
using Application.Features.UserManager.Commands;

namespace Application.Features.UserManager.Commands.ChangePasswordByManager;

public record ChangePasswordByManagerCommand : IRequest<Result<ChangePasswordByManagerResponse>>
{
    public Guid? UserId { get; init; }

    public string? CurrentPassword { get; init; }

    public string? NewPassword { get; init; }

    public string? CurrentUserId { get; init; }
}
