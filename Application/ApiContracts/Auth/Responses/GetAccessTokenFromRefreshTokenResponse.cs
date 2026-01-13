using System;

namespace Application.ApiContracts.Auth.Responses
{
    public class GetAccessTokenFromRefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }
    }
}
