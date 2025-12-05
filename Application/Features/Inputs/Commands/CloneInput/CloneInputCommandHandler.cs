using Application.ApiContracts.Input;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Helpers;
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
    IUnitOfWork unitOfWork) : IRequestHandler<CloneInputCommand, (InputResponse? Data, ErrorResponse? Error)>
{
    public async Task<(InputResponse? Data, ErrorResponse? Error)> Handle(
        CloneInputCommand command,
        CancellationToken cancellationToken)
    {
        var originalInput = await inputReadRepository.GetByIdWithDetailsAsync(
            command.Id,
            cancellationToken,
            DataFetchMode.All)
            .ConfigureAwait(false);

        if(originalInput is null)
        {
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail { Field = "Id", Message = $"Phiếu nhập với Id = {command.Id} không tồn tại" } ]
            });
        }

        var supplier = await supplierReadRepository.GetByIdAsync(
            originalInput.SupplierId ?? 0,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(supplier is null || string.Compare(supplier.StatusId, SupplierStatus.Active) != 0)
        {
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail
                    {
                        Field = "SupplierId",
                        Message = "Nhà cung cấp không tồn tại hoặc không còn hoạt động"
                    } ]
            });
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

        foreach(var originalProduct in originalInput.InputInfos)
        {
            if(!originalProduct.ProductId.HasValue)
            {
                continue;
            }

            if(!variantDict.TryGetValue(originalProduct.ProductId.Value, out var variant))
            {
                continue;
            }

            if(string.Compare(variant.Product?.StatusId, ProductStatus.ForSale) != 0)
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

        if(validProducts.Count == 0)
        {
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail
                    {
                        Field = "Products",
                        Message =
                            "Tất cả sản phẩm trong phiếu nhập gốc đều không còn hợp lệ (đã xoá hoặc không còn bán)"
                    } ]
            });
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

        return (createdInput!.Adapt<InputResponse>(), null);
    }
}
