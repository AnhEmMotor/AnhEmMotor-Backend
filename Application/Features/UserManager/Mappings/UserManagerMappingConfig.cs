using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.UserManager.Mappings;

public sealed class UserManagerMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    { config.NewConfig<ApplicationUser, UserDTOForManagerResponse>().Ignore(dest => dest.Roles!); }
}