using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public sealed class UpdateProductStatusCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductStatusCommand, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        UpdateProductStatusCommand command,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if(product == null)
        {
            return Error.NotFound($"Sản phẩm với Id {command.Id} không tồn tại.");
        }

        product.StatusId = command.StatusId;

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = product.Adapt<ProductDetailForManagerResponse>();
        return response;
    }
}
