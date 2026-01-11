using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.ChangePasswordCurrentUser;

public record ChangePasswordCurrentUserCommand(string? UserId, ChangePasswordRequest Model) : IRequest<Result<ChangePasswordUserByUserResponse>>;
