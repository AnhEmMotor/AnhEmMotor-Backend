using Application.ApiContracts.Client.Auth;
using MediatR;
using System;

namespace Application.Features.Client.Auth
{
    public record VerifyOtpCommand(string PhoneNumber, string OtpCode) : IRequest<AuthResponse>;

    public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, AuthResponse>
    {
        public async Task<AuthResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(
                new AuthResponse("access_token_abc", "refresh_token_xyz", DateTime.UtcNow.AddHours(1)));
        }
    }
}
