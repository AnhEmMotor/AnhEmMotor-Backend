using Application.Interfaces.Repositories.UserManager;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.UserManager
{
    public class UserManagerReadRepository : IUserManagerReadRepository
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
    }
}
