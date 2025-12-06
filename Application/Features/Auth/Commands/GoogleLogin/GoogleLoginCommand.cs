using Application.ApiContracts.Auth.Requests;
using MediatR;

namespace Application.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(GoogleLoginRequest Model) : IRequest;
