using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;

using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInput;

public sealed class UpdateInputCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IInputDeleteRepository deleteRepository,
    ISupplierReadRepository supplierRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputCommand, Result<InputResponse?>>
{
    public async Task<Result<InputResponse?>> Handle(
        UpdateInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(input is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id");
        }

        if(Domain.Constants.InputStatus.IsCannotEdit(input.StatusId))
        {
            if(request.Products.Count != 0)
            {
                return Error.BadRequest("Không được chỉnh sửa sản phẩm trong phiếu nhập đã hoàn thành hoặc đã hủy.", "Products");
            }

            if(request.SupplierId != null)
            {
                return Error.BadRequest("Không được chỉnh sửa mã nhà cung cấp trong phiếu nhập đã hoàn thành hoặc đã hủy.", "Products");
            }
        }

        if(request.SupplierId.HasValue && request.SupplierId != input.SupplierId)
        {
            var supplier = await supplierRepository.GetByIdAsync(
                request.SupplierId.Value,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            if(supplier is null || string.Compare(supplier.StatusId, Domain.Constants.SupplierStatus.Active) != 0)
            {
                return Error.BadRequest("Nhà cung cấp không hợp lệ hoặc không còn hoạt động.", "SupplierId");
            }
        }

        var variantIds = request.Products
            .Where(p => p.ProductId.HasValue)
            .Select(p => p.ProductId!.Value)
            .Distinct()
            .ToList();

        if(variantIds.Count > 0)
        {
            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            var variantsList = variants.ToList();
            if(variantsList.Count != variantIds.Count)
            {
                var foundIds = variantsList.Select(v => v.Id).ToList();
                var missingIds = variantIds.Except(foundIds).ToList();
                return Error.NotFound($"Không tìm thấy {missingIds.Count} sản phẩm: {string.Join(", ", missingIds)}", "Products");
            }

            foreach(var variant in variantsList)
            {
                if(string.Compare(variant.Product?.StatusId, Domain.Constants.ProductStatus.ForSale) != 0)
                {
                    return Error.BadRequest($"Sản phẩm '{variant.Product?.Name ?? variant.Id.ToString()}' không còn được bán.", "Products");
                }
            }
        }

        request.Adapt(input);

        var existingInfoDict = input.InputInfos.ToDictionary(ii => ii.Id);
        var requestInfoDict = request.Products.Where(p => p.Id.HasValue && p.Id > 0).ToDictionary(p => p.Id!.Value);

        var toDelete = input.InputInfos.Where(ii => !requestInfoDict.ContainsKey(ii.Id)).ToList();

        foreach(var info in toDelete)
        {
            deleteRepository.DeleteInputInfo(info);
            input.InputInfos.Remove(info);
        }

        foreach(var productRequest in request.Products)
        {
            if(productRequest.Id.HasValue && productRequest.Id > 0)
            {
                if(existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
                {
                    productRequest.Adapt(existingInfo);
                    if(productRequest.Count.HasValue)
                    {
                        existingInfo.RemainingCount = productRequest.Count.Value;
                    }
                }
            } else
            {
                var newInfo = productRequest.Adapt<InputInfo>();
                newInfo.RemainingCount = newInfo.Count ?? 0;
                input.InputInfos.Add(newInfo);
            }
        }

        updateRepository.Update(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(input.Id, cancellationToken).ConfigureAwait(false);

        return updated!.Adapt<InputResponse>();
    }
}
