using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;

public record ChangeMultipleUsersStatusCommand(ChangeMultipleUsersStatusRequest Model) : IRequest<Result<ChangeStatusMultiUserByManagerResponse>>
{
    public List<Guid>? UserIds { get; init; }
    public string? Status { get; init; }
}