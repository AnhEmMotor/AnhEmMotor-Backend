using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.UpdateUser;

public record UpdateUserCommand(Guid UserId, UpdateUserRequest Model) : IRequest<Result<UserDTOForManagerResponse>>;
