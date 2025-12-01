using Application.ApiContracts.Output;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed class CreateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputInsertRepository insertRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOutputCommand, OutputResponse>
{
    public async Task<OutputResponse> Handle(
        CreateOutputCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Products.Count == 0)
        {
            throw new InvalidOperationException("Đơn hàng phải có ít nhất một sản phẩm.");
        }

        var variantIds = request.Products
            .Where(p => p.ProductId.HasValue)
            .Select(p => p.ProductId!.Value)
            .Distinct()
            .ToList();

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
            }        var output = request.Adapt<Output>();

        if (string.IsNullOrWhiteSpace(output.StatusId))
        {
            output.StatusId = OrderStatus.Pending;
        }

        insertRepository.Add(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await readRepository.GetByIdWithDetailsAsync(
            output.Id,
            cancellationToken)
            .ConfigureAwait(false);

        return created!.Adapt<OutputResponse>();
    }
}
