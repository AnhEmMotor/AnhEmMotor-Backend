using Application.ApiContracts.Input;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Enums;
using Mapster;
using MediatR;
using InputEntity = Domain.Entities.Input;
using InputInfoEntity = Domain.Entities.InputInfo;

namespace Application.Features.Inputs.Commands.CreateInput;

public sealed class CreateInputCommandHandler(
    IInputInsertRepository insertRepository,
    IInputReadRepository readRepository,
    ISupplierReadRepository supplierRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateInputCommand, InputResponse>
{
    public async Task<InputResponse> Handle(
        CreateInputCommand request,
        CancellationToken cancellationToken)
    {
        if(!InputStatus.IsValid(request.StatusId))
        {
            throw new InvalidOperationException($"Trạng thái '{request.StatusId}' không hợp lệ.");
        }

        if(request.SupplierId.HasValue)
        {
            var supplier = await supplierRepository.GetByIdAsync(
                request.SupplierId.Value,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            if(supplier is null)
            {
                throw new InvalidOperationException($"Nhà cung cấp {request.SupplierId} không tồn tại hoặc đã bị xóa.");
            }

            if(supplier.StatusId != SupplierStatus.Active)
            {
                throw new InvalidOperationException($"Nhà cung cấp {supplier.Name} không ở trạng thái 'active'.");
            }
        }

        foreach(var product in request.Products)
        {
            if(product.ProductId.HasValue)
            {
                var variants = await variantRepository.GetByIdAsync(
                    [ product.ProductId.Value ],
                    cancellationToken,
                    DataFetchMode.ActiveOnly)
                    .ConfigureAwait(false);

                var variant = variants.FirstOrDefault();

                if(variant is null)
                {
                    throw new InvalidOperationException($"Sản phẩm {product.ProductId} không tồn tại hoặc đã bị xóa.");
                }

                if(variant.Product?.StatusId != ProductStatus.ForSale)
                {
                    throw new InvalidOperationException($"Sản phẩm {variant.Product?.Name} không ở trạng thái 'for-sale'.");
                }
            }
        }

        var input = request.Adapt<InputEntity>();
        input.InputInfos = request.Products.Select(p =>
        {
            var inputInfo = p.Adapt<InputInfoEntity>();
            inputInfo.RemainingCount = p.Count ?? 0;
            return inputInfo;
        }).ToList();

        insertRepository.Add(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await readRepository.GetByIdWithDetailsAsync(
            input.Id,
            cancellationToken)
            .ConfigureAwait(false);

        return created.Adapt<InputResponse>();
    }
}
