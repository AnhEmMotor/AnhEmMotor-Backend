using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetStoreSettingsForChat;

public class GetStoreSettingsForChatQueryHandler(
    ISettingRepository settingRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetStoreSettingsForChatQuery, Result<ChatToolEnvelope<ChatStoreSettingsDto>>>
{
    private static readonly string[] PublicKeys = [SettingKeys.OrderValueExceeds, SettingKeys.DepositRatio];

    public async Task<Result<ChatToolEnvelope<ChatStoreSettingsDto>>> Handle(
        GetStoreSettingsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var filtered = settings
            .Where(s => s.Key != null && PublicKeys.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(s => s.Key!, s => s.Value, StringComparer.OrdinalIgnoreCase);

        var dto = new ChatStoreSettingsDto
        {
            OrderValueExceeds = filtered.GetValueOrDefault(SettingKeys.OrderValueExceeds),
            DepositRatio = filtered.GetValueOrDefault(SettingKeys.DepositRatio)
        };

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ISettingRepository.GetAllAsync",
            new Dictionary<string, string>(),
            "cai-dat-cua-hang",
            null);

        return ChatToolEnvelope<ChatStoreSettingsDto>.WrapSingle(dto, meta);
    }
}
