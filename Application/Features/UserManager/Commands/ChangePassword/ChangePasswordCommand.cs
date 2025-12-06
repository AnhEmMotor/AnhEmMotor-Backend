using Application.ApiContracts.User.Requests;
using Application.ApiContracts.UserManager.Responses;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangePassword;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Model, string? CurrentUserId) : IRequest<ChangePasswordByManagerResponse>;
