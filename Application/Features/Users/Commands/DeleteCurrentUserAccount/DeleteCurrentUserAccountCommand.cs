using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public record DeleteCurrentUserAccountCommand : IRequest<Result<DeleteUserByUserReponse>>
{
    public string? UserId { get; init; }
}