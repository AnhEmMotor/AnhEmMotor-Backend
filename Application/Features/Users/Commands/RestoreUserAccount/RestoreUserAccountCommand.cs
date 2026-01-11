using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.RestoreUserAccount;

public record RestoreUserAccountCommand(Guid UserId) : IRequest<Result<RestoreUserResponse>>;
