using Application.ApiContracts.Sales.Returns.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Entities;
using MediatR;
using System.Text.Json;

namespace Application.Features.Sales.Returns.Commands.CreateReturnRequest;

public class CreateReturnRequestCommandHandler : IRequestHandler<CreateReturnRequestCommand, Result<ReturnRequestResponse>>
{
    private readonly IReturnRequestWriteRepository _returnRequestRepository;
    private readonly IOutputReadRepository _outputRepository;
    private readonly IProductVariantReadRepository _productVariantReadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReturnRequestCommandHandler(
        IReturnRequestWriteRepository returnRequestRepository,
        IOutputReadRepository outputRepository,
        IProductVariantReadRepository productVariantReadRepository,
        IUnitOfWork unitOfWork)
    {
        _returnRequestRepository = returnRequestRepository;
        _outputRepository = outputRepository;
        _productVariantReadRepository = productVariantReadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReturnRequestResponse>> Handle(CreateReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var order = await _outputRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<ReturnRequestResponse>.Failure("Order not found");
        }

        if (order.StatusId != "completed")
        {
            return Result<ReturnRequestResponse>.Failure("Chỉ áp dụng cho đơn hàng đã giao (completed).");
        }

        // Check if online (heuristic: ProvinceId is not null or ShippingFee > 0)
        if (order.ProvinceId == null && (order.ShippingFee == null || order.ShippingFee == 0))
        {
            return Result<ReturnRequestResponse>.Failure("Chỉ áp dụng cho đơn hàng online.");
        }

        var evidenceImagesJson = "[]";

        if (request.Items == null || request.Items.Count == 0)
        {
            return Result<ReturnRequestResponse>.Failure("Vui lòng chọn ít nhất một mặt hàng cần hoàn.");
        }

        var variantIds = request.Items
            .Where(i => i.VariantId.HasValue && i.VariantId.Value > 0)
            .Select(i => i.VariantId!.Value)
            .Distinct()
            .ToList();

        var variantMap = variantIds.Count > 0
            ? (await _productVariantReadRepository.GetByIdAsync(variantIds, cancellationToken))
                .ToDictionary(v => v.Id)
            : new Dictionary<int, ProductVariant>();

        foreach (var item in request.Items)
        {
            var productId = item.ProductId;
            if (item.VariantId.HasValue && variantMap.TryGetValue(item.VariantId.Value, out var variant))
            {
                productId = variant.ProductId;
            }

            if (productId <= 0)
            {
                return Result<ReturnRequestResponse>.Failure(
                    "Mặt hàng hoàn không hợp lệ: không xác định được sản phẩm từ biến thể.");
            }
        }

        var returnRequest = new ReturnRequest
        {
            OrderId = request.OrderId,
            OrderCode = order.TransactionId ?? order.Id.ToString(),
            OriginalTrackingNumber = request.OriginalTrackingNumber,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Carrier = request.Carrier,
            Type = request.Type,
            Reason = request.Reason,
            Status = "processing",
            EvidenceImagesJson = evidenceImagesJson,
            Items = request.Items.Select(i =>
            {
                var productId = i.ProductId;
                if (i.VariantId.HasValue && variantMap.TryGetValue(i.VariantId.Value, out var variant))
                {
                    productId = variant.ProductId;
                }

                var matchingOutputInfo = order.OutputInfos.FirstOrDefault(oi =>
                    (i.VariantId.HasValue && oi.ProductVariantId == i.VariantId && (!i.ColorId.HasValue || oi.ProductVariantColorId == i.ColorId)) ||
                    (oi.ProductVariant != null && oi.ProductVariant.ProductId == productId));

                var variantId = i.VariantId ?? matchingOutputInfo?.ProductVariantId;
                var colorId = i.ColorId ?? matchingOutputInfo?.ProductVariantColorId;
                var unitPrice = i.UnitPrice > 0 ? i.UnitPrice : (matchingOutputInfo?.Price ?? 0);

                return new ReturnRequestItem
                {
                    ProductId = productId,
                    ProductVariantId = variantId,
                    ProductVariantColorId = colorId,
                    ProductName = !string.IsNullOrWhiteSpace(i.ProductName) ? i.ProductName : (matchingOutputInfo?.ProductVariant?.Product?.Name ?? ""),
                    Quantity = i.Quantity,
                    ReturnQuantity = i.Quantity,
                    UnitPrice = unitPrice,
                    Sku = "",
                };
            }).ToList()
        };

        await _returnRequestRepository.AddAsync(returnRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ReturnRequestResponse
        {
            Id = returnRequest.Id,
            OrderId = returnRequest.OrderId,
            OrderCode = returnRequest.OrderCode,
            CustomerName = returnRequest.CustomerName,
            CustomerPhone = returnRequest.CustomerPhone,
            Carrier = returnRequest.Carrier,
            OriginalTrackingNumber = returnRequest.OriginalTrackingNumber,
            Type = returnRequest.Type,
            Status = returnRequest.Status,
            Reason = returnRequest.Reason,
            Items = returnRequest.Items.Select(i => new ReturnRequestItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                VariantId = i.ProductVariantId,
                ColorId = i.ProductVariantColorId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                ReturnQuantity = i.ReturnQuantity,
                UnitPrice = i.UnitPrice,
                Sku = i.Sku
            }).ToList()
        };

        return Result<ReturnRequestResponse>.Success(response);
    }
}
