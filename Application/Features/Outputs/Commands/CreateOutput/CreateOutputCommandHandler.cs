using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed class CreateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputInsertRepository insertRepository,
    IProductVariantReadRepository variantRepository,
    ISettingRepository settingRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOutputCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        CreateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var variantIds = request.OutputInfos
            .Where(p => p.ProductId.HasValue)
            .Select(p => p.ProductId!.Value)
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
        var errors = new List<Error>();
        for (int i = 0; i < request.OutputInfos.Count; i++)
        {
            var info = request.OutputInfos[i];
            var variant = variantsList.FirstOrDefault(v => v.Id == info.ProductId);
            if (variant?.Product?.ProductCategory != null &&
                variant.Product.ProductCategory.MaxPurchaseQuantity.HasValue)
            {
                var category = variant.Product.ProductCategory;
                var maxAllowed = category.MaxPurchaseQuantity.Value;
                var totalCountForProduct = request.OutputInfos
                    .Where(oi => oi.ProductId == info.ProductId)
                    .Sum(oi => oi.Count ?? 0);
                if (totalCountForProduct > maxAllowed)
                {
                    errors.Add(
                        Error.BadRequest(
                            $"Số lượng mua tối đa cho sản phẩm '{variant.Product.Name}' là {maxAllowed} sản phẩm.",
                            $"products[{i}]"));
                }
            }
        }
        if (errors.Count > 0)
        {
            return Result<OrderDetailResponse>.Failure(errors);
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
        var ratioSetting = settings.FirstOrDefault(
            s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
        if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
        {
            output.DepositRatio = parsedRatio;
        } else
        {
            output.DepositRatio = 50;
        }
        output.BuyerId = request.BuyerId;
        output.CreatedBy = request.BuyerId;
        output.PaymentMethod = request.PaymentMethod ?? PaymentMethod.COD;
        output.PaymentStatus = "Pending";
        insertRepository.Add(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var created = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(created);
        return created.Adapt<OrderDetailResponse>();
    }
}

