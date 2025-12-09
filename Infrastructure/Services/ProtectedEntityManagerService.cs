using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace Infrastructure.Services
{
    public class ProtectedEntityManagerService(IConfiguration configuration) : IProtectedEntityManagerService
    {
        private const string SectionName = "ProtectedAuthorizationEntities";

        public IReadOnlyList<string> GetDefaultRolesForNewUsers()
        { return GetListFromConfig("DefaultRolesForNewUsers"); }

        public IReadOnlyList<string> GetSuperRoles() { return GetListFromConfig("SuperRoles"); }

        public IReadOnlyList<string> GetProtectedUsers() { return GetListFromConfig("ProtectedUsers"); }

        private List<string> GetListFromConfig(string key)
        {
            var fullKey = $"{SectionName}:{key}";

            return configuration.GetSection(fullKey).Get<List<string>>() ?? [];
        }
    }
}
