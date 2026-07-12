using Application.Interfaces.Services.Logistics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Logistics
{
    public class GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger) : IGeocodingService
    {
        public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(
            string address,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AnhEmMotorApp/1.0 (contact@anhemmotor.com)");
                var requestUri = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
                var response = await httpClient.GetAsync(requestUri, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var firstResult = root[0];
                    if (firstResult.TryGetProperty("lat", out var latProp) &&
                        firstResult.TryGetProperty("lon", out var lonProp))
                    {
                        if (double.TryParse(latProp.GetString(), out var lat) &&
                            double.TryParse(lonProp.GetString(), out var lon))
                        {
                            return (lat, lon);
                        }
                    }
                }
                if (address.Contains(','))
                {
                    var parts = address.Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (parts.Length > 1)
                    {
                        var fallbackAddress = string.Join(", ", parts.Skip(1));
                        return await GetCoordinatesAsync(fallbackAddress, cancellationToken);
                    }
                }
            } catch (Exception)
            {
            }
            return null;
        }
    }
}
