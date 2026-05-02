using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using System.Linq;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetOutputsByUserId;

public class GetOutputsByUserIdQueryHandler(
    IOutputReadRepository outputReadRepository,
    ISievePaginator sievePaginator,
    ISettingRepository settingRepository) : IRequestHandler<GetOutputsByUserIdQuery, Result<PagedResult<MyOrderResponse>>>
{
    public async Task<Result<PagedResult<MyOrderResponse>>> Handle(
        GetOutputsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = outputReadRepository.GetQueryable();
        query = query.Where(o => o.BuyerId == request.BuyerId);

        var pagedResult = await sievePaginator.ApplyAsync<OutputEntity, MyOrderResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if(pagedResult.Items?.Any(i => i.DepositRatio == null) == true)
        {
            var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var ratioSetting = settings.FirstOrDefault(
                s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
            var defaultRatio = 50;
            if(ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
            {
                defaultRatio = parsedRatio;
            }

            foreach(var item in pagedResult.Items.Where(i => i.DepositRatio == null))
            {
                item.DepositRatio = defaultRatio;
                if(item.DepositAmount == null)
                    item.DepositAmount = item.Total * (defaultRatio / 100m);
                if(item.RemainingAmount == null)
                    item.RemainingAmount = item.Total - (item.DepositAmount ?? 0);
            }
        }

        return pagedResult;
    }
}
