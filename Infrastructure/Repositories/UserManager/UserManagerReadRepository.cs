using Application.Interfaces.Repositories.UserManager;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.UserManager
{
    public class UserManagerReadRepository(RoleManager<ApplicationRole> roleManager) : IUserManagerReadRepository
    {
        Task<List<ApplicationUser>> IUserManagerReadRepository.GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserManagerReadRepository.IsEmailAvailableAsync(string email, Guid? excludeUserId)
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserManagerReadRepository.IsPhoneNumberAvailableAsync(string? phoneNumber, Guid? excludeUserId)
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserManagerReadRepository.IsUsernameAvailableAsync(string username, Guid? excludeUserId)
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserManagerReadRepository.ValidateAllUsersExistAsync(List<Guid> userIds)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RoleExistsAsync(
            string roleName,
            CancellationToken cancellationToken = default)
        {
            return await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false);
        }
    }
}
