using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public sealed class UpdateProductStatusCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductStatusCommand, (ApiContracts.Product.Responses.ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ApiContracts.Product.Responses.ProductDetailResponse? Data, ErrorResponse? Error)> Handle(
        UpdateProductStatusCommand command,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if(product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Sản phẩm với Id {command.Id} không tồn tại." } ]
            });
        }

        product.StatusId = command.StatusId;

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = product.Adapt<ApiContracts.Product.Responses.ProductDetailResponse>();
        return (response, null);
    }
}
