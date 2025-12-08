using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class ProtectedEntityManagerService(IConfiguration configuration): IProtectedEntityManagerService
    {
        private const string SectionName = "ProtectedAuthorizationEntities";

        public IReadOnlyList<string> GetDefaultRolesForNewUsers()
        {
            return GetListFromConfig("DefaultRolesForNewUsers");
        }

        public IReadOnlyList<string> GetSuperRoles()
        {
            return GetListFromConfig("SuperRoles");
        }

        public IReadOnlyList<string> GetProtectedUsers()
        {
            return GetListFromConfig("ProtectedUsers");
        }

        // Helper private để tránh lặp code
        private List<string> GetListFromConfig(string key)
        {
            var fullKey = $"{SectionName}:{key}";

            // Get<List<string>>() cần package Microsoft.Extensions.Configuration.Binder
            // Nếu null thì trả về list rỗng để tránh NullReferenceException ở tầng Application
            return configuration.GetSection(fullKey).Get<List<string>>() ?? [];
        }
    }
}
