using Application.ApiContracts.DepositSetting.Responses;
using Application.Interfaces.Repositories.DepositSettingHistory;
using MediatR;
using System.Linq;

namespace Application.Features.DepositSettings.Queries;

public record GetDepositSettingsHistoryQuery : IRequest<List<DepositSettingHistoryResponse>>;

public class GetDepositSettingsHistoryQueryHandler(IDepositSettingHistoryRepository historyRepository) : IRequestHandler<GetDepositSettingsHistoryQuery, List<DepositSettingHistoryResponse>>
{
    public async Task<List<DepositSettingHistoryResponse>> Handle(
        GetDepositSettingsHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var history = await historyRepository.GetHistoryAsync(cancellationToken);
        return history.Select(
            x => new DepositSettingHistoryResponse
            {
                Id = x.Id,
                OrderType = x.OrderType,
                OrderThreshold = x.OrderThreshold,
                DepositRatio = x.DepositRatio,
                CreatedAt = x.CreatedAt?.DateTime ?? DateTime.UtcNow,
                CreatedBy = "System"
            })
            .ToList();
    }
}
