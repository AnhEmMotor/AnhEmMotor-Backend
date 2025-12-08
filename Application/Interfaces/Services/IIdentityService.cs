using Application.ApiContracts.Auth.Requests;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<UserAuthDTO> AuthenticateAsync(string usernameOrEmail, string password);
    }
}
