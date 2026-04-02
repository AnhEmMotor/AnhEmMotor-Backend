using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.User;

using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public sealed class UpdateOutputForManagerCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IOutputDeleteRepository deleteRepository,
    IProductVariantReadRepository variantRepository,
    IUserReadRepository userReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputForManagerCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        UpdateOutputForManagerCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id");
        }

        if(request.BuyerId.HasValue && request.BuyerId != output.BuyerId)
        {
            var buyer = await userReadRepository.GetUserByIDAsync(request.BuyerId.Value, cancellationToken)
                .ConfigureAwait(false);
            if(buyer is null)
            {
                return Error.Forbidden(
                    "ID này là 1 tài khoản không tồn tại/đã bị xoá/đã bị cấm. Vui lòng kiểm tra lại.",
                    "BuyerId");
            }

            output.BuyerId = request.BuyerId;
        }

        var variantIds = request.OutputInfos
            .Where(p => p.ProductId.HasValue)
            .Select(p => p.ProductId!.Value)
            .Distinct()
            .ToList();

        List<ProductVariant> variantsList = [];

        if(variantIds.Count > 0)
        {
            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            variantsList = [ .. variants ];

            if(variantsList.Count != variantIds.Count)
            {
                var foundIds = variantsList.Select(v => v.Id).ToList();
                var missingIds = variantIds.Except(foundIds).ToList();
                return Error.NotFound(
                    $"Không tìm thấy {missingIds.Count} sản phẩm: {string.Join(", ", missingIds)}",
                    "Products");
            }

            foreach(var variant in variantsList)
            {
                if(string.Compare(variant.Product?.StatusId, Domain.Constants.Product.ProductStatus.ForSale) != 0)
                {
                    return Error.BadRequest(
                        $"Sản phẩm '{variant.Product?.Name ?? variant.Id.ToString()}' không còn được bán.",
                        "Products");
                }
            }
        }

        if(OrderLockStatus.LockedStatuses.Contains(output.StatusId ?? string.Empty))
        {
            if(request.OutputInfos.Count != output.OutputInfos.Count ||
                request.OutputInfos.Any(ri => !output.OutputInfos.Any(oi => oi.Id == ri.Id)))
            {
                return Error.BadRequest("Đơn hàng đã bị khoá, không thể thay đổi danh sách sản phẩm.", "StatusId");
            }
        }

        if(OrderLockStatus.DeliveryInfoLockedStatuses.Contains(output.StatusId ?? string.Empty))
        {
            if(string.Compare(request.CustomerName, output.CustomerName) != 0 ||
                string.Compare(request.CustomerPhone, output.CustomerPhone) != 0 ||
                string.Compare(request.CustomerAddress, output.CustomerAddress) != 0)
            {
                return Error.BadRequest(
                    "Trạng thái đơn hàng hiện tại không cho phép thay đổi thông tin giao hàng.",
                    "StatusId");
            }
        }

        if(OrderLockStatus.NotesLockedStatuses.Contains(output.StatusId ?? string.Empty))
        {
            if(string.Compare(request.Notes, output.Notes) != 0)
            {
                return Error.BadRequest("Trạng thái đơn hàng hiện tại không cho phép thay đổi ghi chú.", "StatusId");
            }
        }

        request.Adapt(output);

        var existingInfoDict = output.OutputInfos.ToDictionary(oi => oi.Id);

        var requestInfoDict = request.OutputInfos.Where(p => p.Id.HasValue && p.Id > 0).ToDictionary(p => p.Id!.Value);

        var toDelete = output.OutputInfos.Where(oi => !requestInfoDict.ContainsKey(oi.Id)).ToList();

        foreach(var info in toDelete)
        {
            output.OutputInfos.Remove(info);
            deleteRepository.DeleteOutputInfo(info);
        }

        foreach(var productRequest in request.OutputInfos)
        {
            var currentVariant = variantsList.FirstOrDefault(v => v.Id == productRequest.ProductId);

            if(productRequest.Id.HasValue && productRequest.Id > 0)
            {
                if(existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
                {
                    productRequest.Adapt(existingInfo);

                    if(currentVariant != null)
                    {
                        existingInfo.Price = currentVariant.Price;
                    }
                }
            } else
            {
                var newInfo = new OutputInfo
                {
                    ProductVarientId = productRequest.ProductId,
                    Count = productRequest.Count
                };
                if(currentVariant != null)
                {
                    newInfo.Price = currentVariant.Price;
                }
                output.OutputInfos.Add(newInfo);
            }
        }

        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if(string.Compare(output.StatusId, OrderStatus.Completed) == 0 &&
            string.IsNullOrEmpty(output.FinishedBy?.ToString()))
        {
            output.FinishedBy = request.CurrentUserId;
            updateRepository.Update(output);
            await updateRepository.ProcessCOGSForCompletedOrderAsync(output.Id, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);

        return updated.Adapt<OrderDetailResponse>();
    }
}
