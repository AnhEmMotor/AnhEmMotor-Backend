using Application.ApiContracts.User.Responses;
using MediatR;

namespace Application.Features.Users.Commands.RestoreUserAccount;

public record RestoreUserAccountCommand(Guid UserId) : IRequest<RestoreUserResponse>;
