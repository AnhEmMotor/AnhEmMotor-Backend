using Application.ApiContracts.Auth.Requests;
using Application.Common.Models;
using Application.Interfaces.Services;
using Infrastructure.Services.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.Services;

public class ExternalAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IExternalAuthService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<Result<ExternalUserDto>> ValidateGoogleTokenAsync(
        string idToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}",
                cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return Error.Unauthorized("Invalid Google token.");
            }
            var googleUser = await response.Content
                .ReadFromJsonAsync<GoogleUserResponse>(cancellationToken)
                .ConfigureAwait(false);
            if (googleUser is null || string.IsNullOrEmpty(googleUser.Email))
            {
                return Error.Unauthorized("Could not retrieve user info from Google.");
            }
            var clientId = configuration["Authentication:Google:ClientId"];
            if (!string.IsNullOrEmpty(clientId) && string.Compare(googleUser.Audience, clientId) != 0)
            {
                return Error.Unauthorized("Google token audience mismatch.");
            }
            return new ExternalUserDto
            {
                Email = googleUser.Email,
                Name = googleUser.Name ?? googleUser.Email,
                Picture = googleUser.Picture,
                Provider = "Google",
                ProviderId = googleUser.Subject
            };
        } catch (Exception)
        {
            return Error.Failure("An error occurred while validating Google token.");
        }
    }

    public async Task<Result<ExternalUserDto>> ValidateFacebookTokenAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"https://graph.facebook.com/me?access_token={accessToken}&fields=id,name,email,picture",
                cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return Error.Unauthorized("Invalid Facebook token.");
            }
            var facebookUser = await response.Content
                .ReadFromJsonAsync<FacebookUserResponse>(cancellationToken)
                .ConfigureAwait(false);
            if (facebookUser is null || string.IsNullOrEmpty(facebookUser.Email))
            {
                return Error.Unauthorized("Could not retrieve user info from Facebook.");
            }
            return new ExternalUserDto
            {
                Email = facebookUser.Email,
                Name = facebookUser.Name ?? facebookUser.Email,
                Picture = facebookUser.Picture?.Data?.Url,
                Provider = "Facebook",
                ProviderId = facebookUser.Id
            };
        } catch (Exception)
        {
            return Error.Failure("An error occurred while validating Facebook token.");
        }
    }
}
