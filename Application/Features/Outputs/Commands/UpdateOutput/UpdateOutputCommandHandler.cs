using Application.ApiContracts.Output;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed class UpdateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IOutputDeleteRepository deleteRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputCommand, (OutputResponse? Data, ErrorResponse? Error)>
{
    public async Task<(OutputResponse? Data, ErrorResponse? Error)> Handle(
        UpdateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(output is null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Id", Message = $"Không tìm thấy đơn hàng có ID {request.Id}." } ]
            });
        }

        var variantIds = request.OutputInfos
            .Where(p => p.ProductId.HasValue)
            .Select(p => p.ProductId!.Value)
            .Distinct()
            .ToList();

        List<ProductVariant> variantsList = [];

        if(variantIds.Count > 0)
        {
            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            variantsList = [ .. variants ];

            if(variantsList.Count != variantIds.Count)
            {
                var foundIds = variantsList.Select(v => v.Id).ToList();
                var missingIds = variantIds.Except(foundIds).ToList();
                return (null, new ErrorResponse
                {
                    Errors =
                        [ new ErrorDetail
                        {
                            Field = "Products",
                            Message = $"Không tìm thấy {missingIds.Count} sản phẩm: {string.Join(", ", missingIds)}"
                        } ]
                });
            }

            foreach(var variant in variantsList)
            {
                if(string.Compare(variant.Product?.StatusId, Domain.Constants.ProductStatus.ForSale) != 0)
                {
                    return (null, new ErrorResponse
                    {
                        Errors =
                            [ new ErrorDetail
                            {
                                Field = "Products",
                                Message =
                                    $"Sản phẩm '{variant.Product?.Name ?? variant.Id.ToString()}' không còn được bán."
                            } ]
                    });
                }
            }
        }

        request.Adapt(output);

        var existingInfoDict = output.OutputInfos.ToDictionary(oi => oi.Id);

        var requestInfoDict = request.OutputInfos.Where(p => p.Id.HasValue && p.Id > 0).ToDictionary(p => p.Id!.Value);

        var toDelete = output.OutputInfos.Where(oi => !requestInfoDict.ContainsKey(oi.Id)).ToList();

        foreach(var info in toDelete)
        {
            output.OutputInfos.Remove(info);
            deleteRepository.DeleteOutputInfo(info);
        }

        foreach(var productRequest in request.OutputInfos)
        {
            var currentVariant = variantsList.FirstOrDefault(v => v.Id == productRequest.ProductId);

            if(productRequest.Id.HasValue && productRequest.Id > 0)
            {
                if(existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
                {
                    productRequest.Adapt(existingInfo);

                    if(currentVariant != null)
                    {
                        existingInfo.Price = currentVariant.Price;
                    }
                }
            } else
            {
                var newInfo = productRequest.Adapt<OutputInfo>();
                if(currentVariant != null)
                {
                    newInfo.Price = currentVariant.Price;
                }
                output.OutputInfos.Add(newInfo);
            }
        }

        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);

        return (updated!.Adapt<OutputResponse>(), null);
    }
}