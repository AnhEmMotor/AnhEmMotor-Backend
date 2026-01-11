using System;

namespace Application.ApiContracts.Auth.Requests
{
    public class UserAuthDTO
    {
        public Guid Id { get; set; }

        public string? UserName { get; set; }

        public string[]? Roles { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public string? Status { get; set; }

        public string[]? AuthMethods { get; set; }
    }
}
