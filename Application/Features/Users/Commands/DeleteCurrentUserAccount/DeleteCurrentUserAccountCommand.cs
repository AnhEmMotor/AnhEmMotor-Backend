using Application.ApiContracts.User.Responses;
using MediatR;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public record DeleteCurrentUserAccountCommand(string? UserId) : IRequest<DeleteUserByUserReponse>;
