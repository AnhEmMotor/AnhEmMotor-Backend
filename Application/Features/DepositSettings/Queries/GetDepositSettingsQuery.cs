using Application.ApiContracts.DepositSetting.Requests;
using Application.Interfaces.Repositories.Setting;
using MediatR;
using System.Linq;

namespace Application.Features.DepositSettings.Queries;

public record GetDepositSettingsQuery : IRequest<List<DepositSettingItemDto>>;

public class GetDepositSettingsQueryHandler(ISettingRepository settingRepository) : IRequestHandler<GetDepositSettingsQuery, List<DepositSettingItemDto>>
{
    public async Task<List<DepositSettingItemDto>> Handle(
        GetDepositSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var result = new List<DepositSettingItemDto>();
        var defaultTypes = new[] { "Xe máy", "Phụ tùng & xe máy", "Chỉ có phụ tùng", "Chỉ có phụ kiện" };
        var settings = (await settingRepository.GetAllAsync(cancellationToken)).ToList();
        foreach (var type in defaultTypes)
        {
            var thresholdStr = settings.FirstOrDefault(x => x.Key == $"Deposit_{type}_Threshold")?.Value;
            var ratioStr = settings.FirstOrDefault(x => x.Key == $"Deposit_{type}_Ratio")?.Value;
            decimal threshold = decimal.TryParse(thresholdStr, out var t) ? t : 0;
            int ratio = int.TryParse(ratioStr, out var r) ? r : 0;
            result.Add(new DepositSettingItemDto { OrderType = type, OrderThreshold = threshold, DepositRatio = ratio });
        }
        return result;
    }
}
