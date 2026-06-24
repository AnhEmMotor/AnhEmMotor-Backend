using MediatR;
using Application.ApiContracts.Client.Auth;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Application.Features.Client.Auth
{
    public record VerifyOtpCommand(string PhoneNumber, string OtpCode) : IRequest<AuthResponse>;

    public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, AuthResponse>
    {
        public async Task<AuthResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            // Logic:
            // 1. Retrieve OTP from Cache by PhoneNumber
            // 2. Compare with request.OtpCode
            // 3. If valid, generate JWT Access Token & Refresh Token
            return await Task.FromResult(new AuthResponse("access_token_abc", "refresh_token_xyz", DateTime.UtcNow.AddHours(1)));
        }
    }
}
