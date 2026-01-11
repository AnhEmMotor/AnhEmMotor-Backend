using Application.ApiContracts.User.Requests;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangePassword;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Model, string? CurrentUserId) : IRequest<Result<ChangePasswordByManagerResponse>>;
