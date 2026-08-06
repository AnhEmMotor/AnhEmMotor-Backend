using Application.ApiContracts.DepositSetting.Requests;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.DepositSettingHistory;
using Application.Interfaces.Repositories.Setting;
using Domain.Entities;
using MediatR;
using System.Linq;

namespace Application.Features.DepositSettings.Commands;

public record UpdateDepositSettingCommand(UpdateDepositSettingRequest Request) : IRequest<bool>;

public class UpdateDepositSettingCommandHandler(
    ISettingRepository settingRepository,
    IDepositSettingHistoryRepository historyRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateDepositSettingCommand, bool>
{
    public async Task<bool> Handle(UpdateDepositSettingCommand request, CancellationToken cancellationToken)
    {
        var existingSettings = (await settingRepository.GetAllAsync(cancellationToken)).ToList();
        var newSettings = new List<Setting>();
        foreach (var item in request.Request.Settings)
        {
            var thresholdKey = $"Deposit_{item.OrderType}_Threshold";
            var ratioKey = $"Deposit_{item.OrderType}_Ratio";
            var thresholdSetting = existingSettings.FirstOrDefault(x => x.Key == thresholdKey);
            var ratioSetting = existingSettings.FirstOrDefault(x => x.Key == ratioKey);
            bool changed = false;
            if (thresholdSetting == null)
            {
                newSettings.Add(new Setting { Key = thresholdKey, Value = item.OrderThreshold.ToString() });
                changed = true;
            } else if (thresholdSetting.Value != item.OrderThreshold.ToString())
            {
                newSettings.Add(new Setting { Key = thresholdKey, Value = item.OrderThreshold.ToString() });
                changed = true;
            } else
            {
                newSettings.Add(thresholdSetting);
            }
            if (ratioSetting == null)
            {
                newSettings.Add(new Setting { Key = ratioKey, Value = item.DepositRatio.ToString() });
                changed = true;
            } else if (ratioSetting.Value != item.DepositRatio.ToString())
            {
                newSettings.Add(new Setting { Key = ratioKey, Value = item.DepositRatio.ToString() });
                changed = true;
            } else
            {
                newSettings.Add(ratioSetting);
            }
            if (changed)
            {
                historyRepository.Add(
                    new DepositSettingHistory
                    {
                        OrderType = item.OrderType,
                        OrderThreshold = item.OrderThreshold,
                        DepositRatio = item.DepositRatio
                    });
            }
        }
        settingRepository.Update(newSettings);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
