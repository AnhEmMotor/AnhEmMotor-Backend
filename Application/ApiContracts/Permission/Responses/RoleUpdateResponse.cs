using System;

namespace Application.ApiContracts.Permission.Responses
{
    public class RoleUpdateResponse
    {
        public string? Message { get; set; }

        public Guid? RoleId { get; set; }

        public string? RoleName { get; set; }

        public string? Description { get; set; }
    }
}
