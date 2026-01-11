using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public record DeleteCurrentUserAccountCommand(string? UserId) : IRequest<Result<DeleteUserByUserReponse>>;
