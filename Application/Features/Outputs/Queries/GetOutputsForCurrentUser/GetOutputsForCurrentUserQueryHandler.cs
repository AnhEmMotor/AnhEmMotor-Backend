using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using System.Linq;

namespace Application.Features.Outputs.Queries.GetOutputsForCurrentUser;

public sealed class GetOutputsForCurrentUserQueryHandler(
    ICurrentUserContext currentUserContext,
    IOutputReadRepository outputReadRepository,
    ISettingRepository settingRepository) : IRequestHandler<GetOutputsForCurrentUserQuery, Result<PagedResult<MyOrderResponse>>>
{
    public async Task<Result<PagedResult<MyOrderResponse>>> Handle(
        GetOutputsForCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var buyerId = currentUserContext.GetUserId();

        var pagedResult = await outputReadRepository.GetPagedAsync<MyOrderResponse>(
            request.SieveModel!,
            DataFetchMode.ActiveOnly,
            o => o.BuyerId == buyerId,
            cancellationToken)
            .ConfigureAwait(false);

        if (pagedResult.Items?.Any(i => i.DepositRatio == null) == true)
        {
            var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var ratioSetting = settings.FirstOrDefault(
                s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
            var defaultRatio = 50;
            if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
            {
                defaultRatio = parsedRatio;
            }
            foreach (var item in pagedResult.Items.Where(i => i.DepositRatio == null))
            {
                item.DepositRatio = defaultRatio;
                item.DepositAmount ??= item.Total * (defaultRatio / 100m);
                item.RemainingAmount ??= item.Total - (item.DepositAmount ?? 0);
            }
        }

        return pagedResult;
    }
}
