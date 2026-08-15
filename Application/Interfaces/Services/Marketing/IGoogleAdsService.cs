#pragma warning disable IDE0040
namespace Application.Interfaces.Services.Marketing;

public interface IGoogleAdsService
{
    Task<object> GetCampaignPerformanceAsync(CancellationToken cancellationToken);
}
