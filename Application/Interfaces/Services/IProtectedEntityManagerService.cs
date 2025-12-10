using System;

namespace Application.Interfaces.Services
{
    public interface IProtectedEntityManagerService
    {
        public IReadOnlyList<string> GetDefaultRolesForNewUsers();

        public IReadOnlyList<string> GetSuperRoles();

        public IReadOnlyList<string> GetProtectedUsers();
    }
}
