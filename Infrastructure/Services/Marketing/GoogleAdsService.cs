using Application.Interfaces.Services.Marketing;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Marketing;

public class GoogleAdsService(IConfiguration configuration, ILogger<GoogleAdsService> logger) : IGoogleAdsService
{
    private readonly GoogleAdsClient _client = new GoogleAdsClient(new GoogleAdsConfig
    {
        DeveloperToken = configuration["GoogleAds:DeveloperToken"],
        OAuth2ClientId = configuration["GoogleAds:OAuth2ClientId"],
        OAuth2ClientSecret = configuration["GoogleAds:OAuth2ClientSecret"],
        OAuth2RefreshToken = configuration["GoogleAds:OAuth2RefreshToken"]
    });

    public async Task<object> GetCampaignPerformanceAsync(CancellationToken cancellationToken)
    {
        var loginCustomerId = configuration["GoogleAds:LoginCustomerId"];
        logger.LogInformation("Fetching Google Ads data for CustomerId {LoginCustomerId}", loginCustomerId);

        if (string.IsNullOrEmpty(loginCustomerId))
        {
            return new { Error = "Google Ads LoginCustomerId is not configured." };
        }

        try
        {
            var googleAdsServiceClient = _client.GetService(Google.Ads.GoogleAds.Services.V22.GoogleAdsService);
            
            string query = @"
                SELECT 
                    campaign.id, 
                    campaign.name, 
                    metrics.impressions, 
                    metrics.clicks, 
                    metrics.conversions, 
                    metrics.cost_micros 
                FROM campaign 
                WHERE campaign.status = 'ENABLED'";

            var request = new Google.Ads.GoogleAds.V22.Services.SearchGoogleAdsRequest() 
            { 
                CustomerId = loginCustomerId.Replace("-", ""), 
                Query = query 
            };
            
            var response = googleAdsServiceClient.Search(request);

            long totalImpressions = 0;
            long totalClicks = 0;
            double totalConversions = 0;
            long totalCostMicros = 0;
            int campaignsActive = 0;

            foreach (var row in response)
            {
                campaignsActive++;
                totalImpressions += row.Metrics.Impressions;
                totalClicks += row.Metrics.Clicks;
                totalConversions += row.Metrics.Conversions;
                totalCostMicros += row.Metrics.CostMicros;
            }

            return new
            {
                Impressions = totalImpressions,
                Clicks = totalClicks,
                Conversions = totalConversions,
                Cost = totalCostMicros / 1000000.0,
                Currency = "VND",
                CampaignsActive = campaignsActive,
                Status = "Success"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch Google Ads data.");
            return new { Error = "Failed to fetch Google Ads data. Check credentials." };
        }
    }
}
