using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result<ResetPasswordResponse>>;
