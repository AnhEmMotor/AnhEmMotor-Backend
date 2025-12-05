using Application.ApiContracts.User;
using Domain.Entities;
using Mapster;

namespace Application.Features.UserManager.Mappings;

public sealed class UserManagerMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ApplicationUser, UserResponse>()
            .Ignore(dest => dest.Roles);
    }
}