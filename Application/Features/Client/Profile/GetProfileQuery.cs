using Application.ApiContracts.Client.Profile;
using MediatR;

namespace Application.Features.Client.Profile
{
    public record GetProfileQuery() : IRequest<ProfileResponse>;

    public record UpdateProfileCommand(UpdateProfileRequest Request) : IRequest<bool>;

    public class GetProfileHandler : IRequestHandler<GetProfileQuery, ProfileResponse>
    {
        public async Task<ProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(
                new ProfileResponse(
                    "Nguyễn Văn A",
                    "0901234567",
                    "a@gmail.com",
                    "Gold",
                    1500,
                    "123 Biên Hòa",
                    "0907654321"));
        }
    }

    public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, bool>
    {
        public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(true);
        }
    }
}
