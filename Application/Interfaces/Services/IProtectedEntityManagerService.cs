using System;

namespace Application.Interfaces.Services
{
    public interface IProtectedEntityManagerService
    {
        IReadOnlyList<string> GetDefaultRolesForNewUsers();

        IReadOnlyList<string> GetSuperRoles();

        IReadOnlyList<string> GetProtectedUsers();
    }
}
