using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangeUserStatus;

public record ChangeUserStatusCommand : IRequest<Result<ChangeStatusUserByManagerResponse>>
{
    public Guid UserId { get; init; }

    public string? Status { get; init; }
}
