using System;

namespace Application.ApiContracts.UserManager.Responses
{
    public class AssignRoleResponse
    {
        public Guid? Id { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public IEnumerable<string>? Roles { get; set; }
    }
}
