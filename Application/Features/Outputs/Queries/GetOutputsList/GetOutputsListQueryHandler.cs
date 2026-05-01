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

namespace Application.Features.Outputs.Queries.GetOutputsList;

public sealed class GetOutputsListQueryHandler(
    IOutputReadRepository repository, 
    ISievePaginator paginator,
    ISettingRepository settingRepository) : IRequestHandler<GetOutputsListQuery, Result<PagedResult<OutputItemResponse>>>
{
    public async Task<Result<PagedResult<OutputItemResponse>>> Handle(
        GetOutputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        var result = await paginator.ApplyAsync<OutputEntity, OutputItemResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Fallback for legacy orders with null DepositRatio
        if (result.Items?.Any(i => i.DepositRatio == null) == true)
        {
            var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var ratioSetting = settings.FirstOrDefault(s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
            var defaultRatio = 50;
            if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
            {
                defaultRatio = parsedRatio;
            }

            foreach (var item in result.Items.Where(i => i.DepositRatio == null))
            {
                item.DepositRatio = defaultRatio;
                if (item.DepositAmount == null)
                    item.DepositAmount = item.Total * (defaultRatio / 100m);
                if (item.RemainingAmount == null)
                    item.RemainingAmount = item.Total - (item.DepositAmount ?? 0);
            }
        }

        return result;
    }
}
