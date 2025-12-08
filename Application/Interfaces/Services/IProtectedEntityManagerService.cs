using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IProtectedEntityManagerService
    {
        IReadOnlyList<string> GetDefaultRolesForNewUsers();
        IReadOnlyList<string> GetSuperRoles();
        IReadOnlyList<string> GetProtectedUsers();
    }
}
