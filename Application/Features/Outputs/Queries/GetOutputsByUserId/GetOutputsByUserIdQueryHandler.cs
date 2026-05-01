using System.Linq;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
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

        // Fallback for legacy orders with null DepositRatio
        if (pagedResult.Items?.Any(i => i.DepositRatio == null) == true)
        {
            var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var ratioSetting = settings.FirstOrDefault(s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
            var defaultRatio = 50;
            if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
            {
                defaultRatio = parsedRatio;
            }

            foreach (var item in pagedResult.Items.Where(i => i.DepositRatio == null))
            {
                item.DepositRatio = defaultRatio;
                // Note: In list view, we don't recalculate DepositAmount/RemainingAmount here 
                // because it's usually done in mapping. But since mapping already happened,
                // we might need to manually set them if they are null.
                if (item.DepositAmount == null)
                    item.DepositAmount = item.Total * (defaultRatio / 100m);
                if (item.RemainingAmount == null)
                    item.RemainingAmount = item.Total - (item.DepositAmount ?? 0);
            }
        }

        return pagedResult;
    }
}
