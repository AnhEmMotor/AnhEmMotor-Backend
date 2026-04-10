using System;

namespace Application.ApiContracts.Auth.Requests
{
    public record ExternalUserDto
    {
        public required string Email { get; init; }

        public required string Name { get; init; }

        public string? Picture { get; init; }

        public required string Provider { get; init; }

        public required string ProviderId { get; init; }
    }
}
