using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Services;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputForCurrentUserById;

public sealed class GetOutputForCurrentUserByIdQueryHandler(
    ICurrentUserContext currentUserContext,
    IOutputReadRepository repository,
    ISettingRepository settingRepository) : IRequestHandler<GetOutputForCurrentUserByIdQuery, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        GetOutputForCurrentUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var output = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng {request.Id}.", nameof(request.Id));
        }
        if (output.BuyerId != currentUserContext.GetUserId())
        {
            return Error.Forbidden("Bạn không có quyền xem đơn hàng này.");
        }
        if (output.DepositRatio == null)
        {
            var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var ratioSetting = settings.FirstOrDefault(
                setting => string.Equals(setting.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
            if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
            {
                output.DepositRatio = parsedRatio;
            }
        }
        return output.Adapt<OrderDetailResponse>();
    }
}
