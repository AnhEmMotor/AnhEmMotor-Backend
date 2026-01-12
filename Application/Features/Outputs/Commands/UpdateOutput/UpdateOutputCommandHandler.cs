using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;

using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed class UpdateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IOutputDeleteRepository deleteRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputCommand, Result<OutputResponse?>>
{
    public async Task<Result<OutputResponse?>> Handle(
        UpdateOutputCommand request,
        CancellationToken cancellationToken)
    {
        if(request.Model.CurrentUserId is null)
        {
            return Error.BadRequest("CurrentUserId không được để trống.", "CurrentUserId");
        }

        var output = await readRepository.GetByIdWithDetailsAsync(
            request.Model.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id");
        }

        if(output.Buyer is null)
        {
            return Error.BadRequest("Đơn hàng không có thông tin người mua. Vui lòng kiểm tra lại", "Buyer");
        }

        if(output.Buyer.Id != request.Model.CurrentUserId)
        {
            return Error.Unauthorized("Người dùng hiện tại không có quyền cập nhật đơn hàng này.", "CurrentUserId");
        }

        var variantIds = request.Model.OutputInfos
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

        request.Adapt(output);

        var existingInfoDict = output.OutputInfos.ToDictionary(oi => oi.Id);

        var requestInfoDict = request.Model.OutputInfos.Where(p => p.Id.HasValue && p.Id > 0).ToDictionary(p => p.Id!.Value);

        var toDelete = output.OutputInfos.Where(oi => !requestInfoDict.ContainsKey(oi.Id)).ToList();

        foreach(var info in toDelete)
        {
            output.OutputInfos.Remove(info);
            deleteRepository.DeleteOutputInfo(info);
        }

        foreach(var productRequest in request.Model.OutputInfos)
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

        return updated.Adapt<OutputResponse>();
    }
}