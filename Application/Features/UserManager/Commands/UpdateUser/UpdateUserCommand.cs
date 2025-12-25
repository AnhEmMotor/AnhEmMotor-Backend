using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using MediatR;

namespace Application.Features.UserManager.Commands.UpdateUser;

public record UpdateUserCommand(Guid UserId, UpdateUserRequest Model) : IRequest<UserDTOForManagerResponse>;
