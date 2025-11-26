using Application.ApiContracts.Input;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInput;

public sealed class UpdateInputCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IInputDeleteRepository deleteRepository,
    ISupplierReadRepository supplierRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputCommand, InputResponse>
{
    public async Task<InputResponse> Handle(
        UpdateInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if (input is null)
        {
            throw new InvalidOperationException($"Không tìm thấy phiếu nhập có ID {request.Id}.");
        }

        if (!string.IsNullOrWhiteSpace(request.StatusId) && !Domain.Constants.InputStatus.IsValid(request.StatusId))
        {
            throw new InvalidOperationException($"Trạng thái '{request.StatusId}' không hợp lệ.");
        }

        if (request.SupplierId.HasValue && request.SupplierId != input.SupplierId)
        {
            var supplier = await supplierRepository.GetByIdAsync(
                request.SupplierId.Value,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            if (supplier is null || supplier.StatusId != Domain.Constants.SupplierStatus.Active)
            {
                throw new InvalidOperationException("Nhà cung cấp không hợp lệ hoặc không còn hoạt động.");
            }
        }

        var variantIds = request.Products
            .Where(p => p.ProductId.HasValue)
            .Select(p => p.ProductId!.Value)
            .Distinct()
            .ToList();

        if (variantIds.Count > 0)
        {
            var variants = await variantRepository.GetByIdAsync(
                variantIds,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            var variantsList = variants.ToList();
            if (variantsList.Count != variantIds.Count)
            {
                var foundIds = variantsList.Select(v => v.Id).ToList();
                var missingIds = variantIds.Except(foundIds).ToList();
                throw new InvalidOperationException(
                    $"Không tìm thấy {missingIds.Count} sản phẩm: {string.Join(", ", missingIds)}");
            }

            foreach (var variant in variantsList)
            {
                if (variant.Product?.StatusId != Domain.Constants.ProductStatus.ForSale)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm '{variant.Product?.Name ?? variant.Id.ToString()}' không còn được bán.");
                }
            }
        }

        request.Adapt(input);

        var existingInfoDict = input.InputInfos.ToDictionary(ii => ii.Id);
        var requestInfoDict = request.Products
            .Where(p => p.Id.HasValue && p.Id > 0)
            .ToDictionary(p => p.Id!.Value);

        var toDelete = input.InputInfos
            .Where(ii => !requestInfoDict.ContainsKey(ii.Id))
            .ToList();

        foreach (var info in toDelete)
        {
            deleteRepository.DeleteInputInfo(info);
            input.InputInfos.Remove(info);
        }

        foreach (var productRequest in request.Products)
        {
            if (productRequest.Id.HasValue && productRequest.Id > 0)
            {
                if (existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
                {
                    productRequest.Adapt(existingInfo);
                    if (productRequest.Count.HasValue)
                    {
                        existingInfo.RemainingCount = productRequest.Count.Value;
                    }
                }
            }
            else
            {
                var newInfo = productRequest.Adapt<InputInfo>();
                newInfo.RemainingCount = newInfo.Count ?? 0;
                input.InputInfos.Add(newInfo);
            }
        }

        updateRepository.Update(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(
            input.Id,
            cancellationToken)
            .ConfigureAwait(false);

        return updated!.Adapt<InputResponse>();
    }
}
