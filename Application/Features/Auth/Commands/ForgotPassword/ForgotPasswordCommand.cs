using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result<ForgotPasswordResponse>>;
