using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed class CreateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputInsertRepository insertRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOutputCommand, (OutputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(OutputResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        CreateOutputCommand request,
        CancellationToken cancellationToken)
    {
        if(request.OutputInfos.Count == 0)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Products",
                        Message = "Đơn hàng phải có ít nhất một sản phẩm."
                    } ]
            });
        }

        var variantIds = request.OutputInfos
            .Where(p => p.ProductId.HasValue)
            .Select(p => p.ProductId!.Value)
            .Distinct()
            .ToList();

        var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        var variantsList = variants.ToList();

        if(variantsList.Count != variantIds.Count)
        {
            var foundIds = variantsList.Select(v => v.Id).ToList();
            var missingIds = variantIds.Except(foundIds).ToList();
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
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
                return (null, new Common.Models.ErrorResponse
                {
                    Errors =
                        [ new Common.Models.ErrorDetail
                        {
                            Field = "Products",
                            Message = $"Sản phẩm '{variant.Product?.Name ?? variant.Id.ToString()}' không còn được bán."
                        } ]
                });
            }
        }
        var output = request.Adapt<Output>();

        foreach(var info in output.OutputInfos)
        {
            var matchingVariant = variantsList.FirstOrDefault(v => v.Id == info.ProductVarientId);
            if(matchingVariant != null)
            {
                info.Price = matchingVariant.Price;
            }
        }

        if(string.IsNullOrWhiteSpace(output.StatusId))
        {
            output.StatusId = OrderStatus.Pending;
        }

        output.BuyerId = request.CurrentUserId;

        insertRepository.Add(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);

        return (created!.Adapt<OutputResponse>(), null);
    }
}
