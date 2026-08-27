using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using Domain.Constants.Product;
using Mapster;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Outputs.Queries.GetReturnableOutputById;

/// <summary>
/// Handler xử lý query lấy chi tiết đơn hàng cho việc hoàn trả hàng,
/// chỉ giữ lại những sản phẩm không phải là xe máy (không quản lý theo số VIN) và chưa được gán mã VIN.
/// </summary>
public class GetReturnableOutputByIdQueryHandler(
    IOutputReadRepository repository,
    ISettingRepository settingRepository) : IRequestHandler<GetReturnableOutputByIdQuery, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        GetReturnableOutputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var output = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (output is null)
        {
            return Error.NotFound($"Output with Id {request.Id} not found.", nameof(request.Id));
        }

        // Lọc các sản phẩm đủ điều kiện hoàn hàng:
        // 1. Không phải là xe máy (ManagementType != "vin_number")
        // 2. Chưa gán mã VIN (Vehicles rỗng hoặc không có xe nào được gán)
        var returnableInfos = output.OutputInfos
            .Where(oi => oi.DeletedAt == null &&
                         (oi.ProductVariant == null ||
                          oi.ProductVariant.Product == null ||
                          oi.ProductVariant.Product.ProductCategory == null ||
                          !string.Equals(oi.ProductVariant.Product.ProductCategory.ManagementType, ProductManagementType.VinNumber, StringComparison.OrdinalIgnoreCase)) &&
                         (oi.Vehicles == null || oi.Vehicles.Count == 0))
            .ToList();

        output.OutputInfos = returnableInfos;

        if (output.DepositRatio == null)
        {
            var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var ratioSetting = settings.FirstOrDefault(
                s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
            if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
            {
                output.DepositRatio = parsedRatio;
            }
        }

        return output.Adapt<OrderDetailResponse>();
    }
}
