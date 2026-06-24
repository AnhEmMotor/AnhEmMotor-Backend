using MediatR;
using Application.ApiContracts.Client.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Client.Auth
{
    public record LoginPhoneCommand(string PhoneNumber) : IRequest<bool>;

    public class LoginPhoneHandler : IRequestHandler<LoginPhoneCommand, bool>
    {
        // Mock OTP Service
        public async Task<bool> Handle(LoginPhoneCommand request, CancellationToken cancellationToken)
        {
            // Logic: 
            // 1. Check if user exists by phone
            // 2. Generate 6-digit OTP
            // 3. Save OTP to Cache/DB with expiry (5 mins)
            // 4. Send SMS via Provider
            return await Task.FromResult(true);
        }
    }
}
