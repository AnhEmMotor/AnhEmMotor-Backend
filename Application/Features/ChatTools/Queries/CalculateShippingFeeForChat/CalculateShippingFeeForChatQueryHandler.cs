using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Services.Shipping;
using Application.Interfaces.Services.Shipping.Models;
using MediatR;

namespace Application.Features.ChatTools.Queries.CalculateShippingFeeForChat;

public class CalculateShippingFeeForChatQueryHandler(
    IShippingService shippingService,
    IProductVariantReadRepository variantReadRepository,
    IProductReadRepository productReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<CalculateShippingFeeForChatQuery, Result<ChatToolEnvelope<ChatShippingFeeDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatShippingFeeDto>>> Handle(
        CalculateShippingFeeForChatQuery request,
        CancellationToken cancellationToken)
    {
        var variant = await variantReadRepository
            .GetByIdWithDetailsAsync(request.ProductVariantId, cancellationToken)
            .ConfigureAwait(false);
        if (variant == null)
        {
            return Result<ChatToolEnvelope<ChatShippingFeeDto>>.Failure(
                Error.NotFound("Không tìm thấy biến thể sản phẩm"));
        }
        var product = await productReadRepository.GetByIdWithDetailsAsync(variant.ProductId, cancellationToken)
            .ConfigureAwait(false);
        if (product == null)
        {
            return Result<ChatToolEnvelope<ChatShippingFeeDto>>.Failure(Error.NotFound("Không tìm thấy sản phẩm"));
        }
        var weight = variant.Weight > 0 ? variant.Weight : (product.Weight > 0 ? product.Weight : 1000);
        var length = variant.Length > 0 ? variant.Length : (product.Length > 0 ? product.Length : 10);
        var width = variant.Width > 0 ? variant.Width : (product.Width > 0 ? product.Width : 10);
        var height = variant.Height > 0 ? variant.Height : (product.Height > 0 ? product.Height : 10);
        var calculateRequest = new CalculateShippingFeeRequest
        {
            ToWardIdV2 = int.TryParse(request.WardId, out var wardId) ? wardId : 0,
            ToAddressV2 = string.Empty,
            IsNewToAddress = true,
            ToWardCode = request.WardId,
            Items =
                [new ShippingItemDto
                {
                    Name = variant.VariantName ?? product.Name ?? string.Empty,
                    Quantity = request.Quantity,
                    Weight = (int)weight!,
                    Length = (int)length!,
                    Width = (int)width!,
                    Height = (int)height!
                }]
        };
        var feeResult = await shippingService.CalculateShippingFeeAsync(calculateRequest, cancellationToken)
            .ConfigureAwait(false);
        if (feeResult.IsFailure)
        {
            return Result<ChatToolEnvelope<ChatShippingFeeDto>>.Failure(feeResult.Errors);
        }
        var dto = new ChatShippingFeeDto { Fee = feeResult.Value };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IShippingService.CalculateShippingFeeAsync",
            new Dictionary<string, string>
            {
                ["Sản phẩm"] = variant.VariantName ?? product.Name ?? string.Empty,
                ["Số lượng"] = request.Quantity.ToString(),
                ["Phường/Xã"] = request.WardId
            },
            "phi-van-chuyen",
            "VND");
        return ChatToolEnvelope<ChatShippingFeeDto>.WrapSingle(dto, meta);
    }
}
