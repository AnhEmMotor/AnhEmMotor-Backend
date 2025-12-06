using System;

namespace Application.ApiContracts.Auth.Responses
{
    public class GetAccessTokenFromRefreshTokenResponse
    {
/// <summary>
        /// Access Token (JWT)
        /// </summary>
                public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian hết hạn của Access Token
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
