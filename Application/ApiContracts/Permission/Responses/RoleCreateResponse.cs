using System;

namespace Application.ApiContracts.Permission.Responses
{
    public class RoleCreateResponse
    {
        public string? Message { get; set; }

        public Guid? RoleId { get; set; }

        public string? RoleName { get; set; }

        public string? Description { get; set; }

        public List<string>? Permissions { get; set; }
    }
}
