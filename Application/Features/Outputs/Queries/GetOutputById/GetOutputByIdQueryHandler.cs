using System.Linq;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputById;

public sealed class GetOutputByIdQueryHandler(
    IOutputReadRepository repository,
    ISettingRepository settingRepository) : IRequestHandler<GetOutputByIdQuery, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        GetOutputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var output = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(output is null)
        {
            return Error.NotFound($"Output with Id {request.Id} not found.", nameof(request.Id));
        }

        if (output.DepositRatio == null)
        {
            var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var ratioSetting = settings.FirstOrDefault(s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
            if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
            {
                output.DepositRatio = parsedRatio;
            }
            else
            {
                output.DepositRatio = 50;
            }
        }

        return output.Adapt<OrderDetailResponse>();
    }
}
