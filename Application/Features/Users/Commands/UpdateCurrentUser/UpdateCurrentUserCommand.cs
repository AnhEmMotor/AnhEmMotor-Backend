using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand(string? UserId, UpdateUserRequest Model) : IRequest<Result<UserDTOForManagerResponse>>;
