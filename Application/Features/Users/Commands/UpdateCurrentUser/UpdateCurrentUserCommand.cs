using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using MediatR;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand(string? UserId, UpdateUserRequest Model) : IRequest<UserDTOForManagerResponse>;
