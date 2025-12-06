using System;

namespace Application.ApiContracts.Permission.Responses
{
    public class PermissionAndRoleOfUserResponse
    {
        public Guid? UserId { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public IList<string>? Roles { get; set; }

        public List<PermissionResponse>? Permissions { get; set; }
    }
}
