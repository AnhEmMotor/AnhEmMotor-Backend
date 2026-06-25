using MediatR;

namespace Application.Features.Client.Auth
{
    public record LoginPhoneCommand(string PhoneNumber) : IRequest<bool>;

    public class LoginPhoneHandler : IRequestHandler<LoginPhoneCommand, bool>
    {
        public async Task<bool> Handle(LoginPhoneCommand request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(true);
        }
    }
}
