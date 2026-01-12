using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;

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
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOutputCommand, Result<OutputResponse?>>
{
    public async Task<Result<OutputResponse?>> Handle(CreateOutputCommand request, CancellationToken cancellationToken)
    {
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
            return Error.NotFound(
                $"Không tìm th?y {missingIds.Count} s?n ph?m: {string.Join(", ", missingIds)}",
                "Products");
        }

        foreach(var variant in variantsList)
        {
            if(string.Compare(variant.Product?.StatusId, Domain.Constants.ProductStatus.ForSale) != 0)
            {
                return Error.BadRequest(
                    $"S?n ph?m '{variant.Product?.Name ?? variant.Id.ToString()}' không còn du?c bán.",
                    "Products");
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

        output.BuyerId = request.BuyerId;

        insertRepository.Add(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);

        return created.Adapt<OutputResponse>();
    }
}

