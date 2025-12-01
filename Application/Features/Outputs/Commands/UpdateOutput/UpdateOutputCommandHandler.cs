using Application.ApiContracts.Output;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed class UpdateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IOutputDeleteRepository deleteRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputCommand, OutputResponse>
{
    public async Task<OutputResponse> Handle(
        UpdateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if (output is null)
        {
            throw new InvalidOperationException($"Không tìm thấy đơn hàng có ID {request.Id}.");
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

        request.Adapt(output);

        var existingInfoDict = output.OutputInfos.ToDictionary(oi => oi.Id);
        var requestInfoDict = request.Products
            .Where(p => p.Id.HasValue && p.Id > 0)
            .ToDictionary(p => p.Id!.Value);

        var toDelete = output.OutputInfos
            .Where(oi => !requestInfoDict.ContainsKey(oi.Id))
            .ToList();

        foreach (var info in toDelete)
        {
            deleteRepository.DeleteOutputInfo(info);
            output.OutputInfos.Remove(info);
        }

        foreach (var productRequest in request.Products)
        {
            if (productRequest.Id.HasValue && productRequest.Id > 0)
            {
                if (existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
                {
                    productRequest.Adapt(existingInfo);
                }
            }
            else
            {
                var newInfo = productRequest.Adapt<OutputInfo>();
                output.OutputInfos.Add(newInfo);
            }
        }

        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(
            output.Id,
            cancellationToken)
            .ConfigureAwait(false);

        return updated!.Adapt<OutputResponse>();
    }
}
