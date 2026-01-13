using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;

using Domain.Constants;
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
    IUnitOfWork unitOfWork) : IRequestHandler<CreateInputCommand, Result<InputResponse?>>
{
    public async Task<Result<InputResponse?>> Handle(CreateInputCommand request, CancellationToken cancellationToken)
    {
        if(request.SupplierId.HasValue)
        {
            var supplier = await supplierRepository.GetByIdAsync(
                request.SupplierId.Value,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            if(supplier is null)
            {
                return Error.NotFound($"Nhà cung cấp {request.SupplierId} không tồn tại hoặc đã bị xóa.", "SupplierId");
            }

            if(string.Compare(supplier.StatusId, SupplierStatus.Active) != 0)
            {
                return Error.BadRequest($"Nhà cung cấp {supplier.Name} không ở trạng thái 'active'.", "SupplierId");
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
                    return Error.BadRequest($"Sản phẩm {product.ProductId} không tồn tại hoặc đã bị xóa.", "Products");
                }
            }
        }

        var input = request.Adapt<InputEntity>();
        input.StatusId = Domain.Constants.Input.InputStatus.Working;
        input.InputInfos = [ .. request.Products
            .Select(
                p =>
                {
                    var inputInfo = p.Adapt<InputInfoEntity>();
                    inputInfo.RemainingCount = p.Count ?? 0;
                    return inputInfo;
                }) ];

        insertRepository.Add(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await readRepository.GetByIdWithDetailsAsync(input.Id, cancellationToken).ConfigureAwait(false);

        return created!.Adapt<InputResponse>();
    }
}
