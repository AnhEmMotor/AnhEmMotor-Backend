using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Setting;

using MediatR;
using SettingEntity = Domain.Entities.Setting;

namespace Application.Features.Settings.Commands.SetSettings;

public sealed class SetSettingsCommandHandler(ISettingRepository settingRepository, IUnitOfWork unitOfWork) : IRequestHandler<SetSettingsCommand, Result<Dictionary<string, string?>?>>
{
    public async Task<Result<Dictionary<string, string?>?>> Handle(
        SetSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var settingsToUpsert = request.Settings.Select(req => new SettingEntity { Key = req.Key, Value = req.Value });

        settingRepository.Update(settingsToUpsert);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return request.Settings;
    }
}
