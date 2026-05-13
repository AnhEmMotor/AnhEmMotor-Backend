using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Constants.Input;
using Domain.Constants.Product;
using Mapster;
using MediatR;
using InputEntity = Domain.Entities.Input;
using InputInfoEntity = Domain.Entities.InputInfo;

namespace Application.Features.Inputs.Commands.CloneInput;

public sealed class CloneInputCommandHandler(
    IInputReadRepository inputReadRepository,
    IInputInsertRepository inputInsertRepository,
    ISupplierReadRepository supplierReadRepository,
    IProductVariantReadRepository variantReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CloneInputCommand, Result<InputDetailResponse?>>
{
    public async Task<Result<InputDetailResponse?>> Handle(
        CloneInputCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Id.HasValue)
        {
            return Error.BadRequest("Id không du?c d? tr?ng", "Id");
        }
        var originalInput = await inputReadRepository.GetByIdWithDetailsAsync(
            command.Id.Value,
            cancellationToken,
            DataFetchMode.All)
            .ConfigureAwait(false);
        if (originalInput is null)
        {
            return Error.NotFound($"Phi?u nh?p v?i Id = {command.Id.Value} không t?n t?i", "Id");
        }
        var supplier = await supplierReadRepository.GetByIdAsync(
            originalInput.SupplierId ?? 0,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (supplier is null || string.Compare(supplier.StatusId, SupplierStatus.Active) != 0)
        {
            return Error.BadRequest("Nhà cung c?p không t?n t?i ho?c không còn ho?t d?ng", "SupplierId");
        }
        var productVariantIds = originalInput.InputInfos
            .Where(p => p.ProductId.HasValue)
            .Select(p => p.ProductId!.Value)
            .Distinct()
            .ToList();
        var variants = await variantReadRepository.GetByIdAsync(
            productVariantIds,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        var variantDict = variants.ToDictionary(v => v.Id);
        var validProducts = new List<InputInfoEntity>();
        foreach (var originalProduct in originalInput.InputInfos)
        {
            if (!originalProduct.ProductId.HasValue)
            {
                continue;
            }
            if (!variantDict.TryGetValue(originalProduct.ProductId.Value, out var variant))
            {
                continue;
            }
            if (string.Compare(variant.Product?.StatusId, ProductStatus.ForSale) != 0)
            {
                continue;
            }
            validProducts.Add(
                new InputInfoEntity
                {
                    ProductId = originalProduct.ProductId,
                    Count = originalProduct.Count,
                    RemainingCount = originalProduct.Count,
                    InputPrice = originalProduct.InputPrice,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
        }
        if (validProducts.Count == 0)
        {
            return Error.BadRequest(
                "T?t c? s?n ph?m trong phi?u nh?p g?c d?u không còn h?p l? (dã xoá ho?c không còn bán)",
                "Products");
        }
        var newInput = new InputEntity
        {
            Notes = originalInput.Notes,
            SupplierId = originalInput.SupplierId,
            StatusId = InputStatus.Working,
            InputInfos = validProducts,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        inputInsertRepository.Add(newInput);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var createdInput = await inputReadRepository.GetByIdWithDetailsAsync(newInput.Id, cancellationToken)
            .ConfigureAwait(false);
        return createdInput!.Adapt<InputDetailResponse>();
    }
}

