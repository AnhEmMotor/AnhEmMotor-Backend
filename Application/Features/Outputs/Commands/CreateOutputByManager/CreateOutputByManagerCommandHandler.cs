using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutputByManager;

public sealed class CreateOutputByManagerCommandHandler(
    IOutputReadRepository readRepository,
    IOutputInsertRepository insertRepository,
    IOutputUpdateRepository updateRepository,
    IProductVariantReadRepository variantRepository,
    IUserReadRepository userReadRepository,
    ISettingRepository settingRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOutputByManagerCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        CreateOutputByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var userData = await userReadRepository.GetUserByIDAsync(request.BuyerId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if (userData == null)
        {
            return Error.Forbidden(
                "ID này là 1 tài khoản không tồn tại/đã bị xoá/đã bị cấm. Vui lòng kiểm tra lại.",
                "BuyerId");
        }
        var variantIds = request.OutputInfos
            .Where(p => p.ProductVarientId.HasValue)
            .Select(p => p.ProductVarientId!.Value)
            .Distinct()
            .ToList();
        var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        var variantsList = variants.ToList();
        if (variantsList.Count != variantIds.Count)
        {
            var foundIds = variantsList.Select(v => v.Id).ToList();
            var missingIds = variantIds.Except(foundIds).ToList();
            return Error.NotFound(
                $"Không tìm thấy {missingIds.Count} sản phẩm: {string.Join(", ", missingIds)}",
                "Products");
        }
        foreach (var variant in variantsList)
        {
            if (string.Compare(variant.Product?.StatusId, Domain.Constants.Product.ProductStatus.ForSale) != 0)
            {
                return Error.BadRequest(
                    $"Sản phẩm '{variant.Product?.Name ?? variant.Id.ToString()}' không còn được bán.",
                    "Products");
            }
        }
        foreach (var product in request.OutputInfos.Where(p => p.ProductVarientId.HasValue))
        {
            var variant = variantsList.First(v => v.Id == product.ProductVarientId!.Value);
            var colorValidation = ValidateVariantColor(variant, product.ProductVarientColorId);
            if (colorValidation is not null)
            {
                return colorValidation;
            }
        }
        var output = request.Adapt<Output>();
        foreach (var info in output.OutputInfos)
        {
            var matchingVariant = variantsList.FirstOrDefault(v => v.Id == info.ProductVarientId);
            if (matchingVariant != null)
            {
                info.Price = matchingVariant.Price;
            }
        }
        var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (request.DepositRatio.HasValue)
        {
            output.DepositRatio = request.DepositRatio.Value;
        } else
        {
            var ratioSetting = settings.FirstOrDefault(
                s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
            if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
            {
                output.DepositRatio = parsedRatio;
            }
        }
        if (string.IsNullOrWhiteSpace(output.StatusId))
        {
            var totalPrice = output.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0));
            var thresholdSetting = settings.FirstOrDefault(
                s => string.Equals(s.Key, SettingKeys.OrderValueExceeds, StringComparison.OrdinalIgnoreCase));
            decimal threshold = 100000000;
            if (thresholdSetting != null && decimal.TryParse(thresholdSetting.Value, out var parsedThreshold))
            {
                threshold = parsedThreshold;
            }
            output.StatusId = totalPrice >= threshold ? OrderStatus.WaitingDeposit : OrderStatus.Pending;
        }
        insertRepository.Add(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (string.Compare(output.StatusId, OrderStatus.Completed) == 0)
        {
            output.FinishedBy = request.CurrentUserId;
            updateRepository.Update(output);
            var result = await updateRepository.HandleInventoryTransactionAsync(output.Id, true, cancellationToken)
                .ConfigureAwait(false);
            if (result.IsFailure)
            {
                return Result<OrderDetailResponse>.Failure(result.Errors!);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        } else if (string.Compare(output.StatusId, OrderStatus.Delivering) == 0)
        {
            var result = await updateRepository.HandleInventoryTransactionAsync(output.Id, false, cancellationToken)
                .ConfigureAwait(false);
            if (result.IsFailure)
            {
                return Result<OrderDetailResponse>.Failure(result.Errors!);
            }
        }
        var created = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(created);
        return created.Adapt<OrderDetailResponse>();
    }

    private static Error? ValidateVariantColor(ProductVariant variant, int? productVarientColorId)
    {
        if (variant.ProductVariantColors.Count == 0)
        {
            return productVarientColorId.HasValue
                ? Error.BadRequest("Biến thể sản phẩm này không có màu sắc để chọn.", "ProductVarientColorId")
                : null;
        }
        if (!productVarientColorId.HasValue || productVarientColorId <= 0)
        {
            return Error.BadRequest("Biến thể sản phẩm có màu sắc, ProductVarientColorId là bắt buộc.", "ProductVarientColorId");
        }
        return variant.ProductVariantColors.Any(c => c.Id == productVarientColorId.Value)
            ? null
            : Error.BadRequest("ProductVarientColorId không thuộc biến thể sản phẩm đã chọn.", "ProductVarientColorId");
    }
}

