using Application.ApiContracts.Auth.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(GoogleLoginRequest Model) : IRequest<Result>;
