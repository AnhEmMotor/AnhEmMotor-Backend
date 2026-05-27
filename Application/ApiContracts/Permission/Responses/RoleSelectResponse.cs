using System;

namespace Application.ApiContracts.Permission.Responses
{
    public class RoleSelectResponse
    {
        public Guid? ID { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
