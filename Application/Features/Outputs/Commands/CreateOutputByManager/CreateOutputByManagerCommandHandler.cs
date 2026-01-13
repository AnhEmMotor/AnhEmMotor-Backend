using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.User;

using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutputByManager;

public sealed class CreateOutputByManagerCommandHandler(
    IOutputReadRepository readRepository,
    IOutputInsertRepository insertRepository,
    IProductVariantReadRepository variantRepository,
    IUserReadRepository userReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOutputByManagerCommand, Result<OutputResponse?>>
{
    public async Task<Result<OutputResponse?>> Handle(
        CreateOutputByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var userData = await userReadRepository.GetUserByIDAsync(request.BuyerId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if(userData == null)
        {
            return Error.Forbidden("ID này là 1 tài khoản không tồn tại/đã bị xoá/đã bị cấm. Vui lòng kiểm tra lại.", "BuyerId");
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
            return Error.NotFound($"Không tìm thấy {missingIds.Count} sản phẩm: {string.Join(", ", missingIds)}", "Products");
        }

        foreach(var variant in variantsList)
        {
            if(string.Compare(variant.Product?.StatusId, Domain.Constants.ProductStatus.ForSale) != 0)
            {
                return Error.BadRequest($"Sản phẩm '{variant.Product?.Name ?? variant.Id.ToString()}' không còn được bán.", "Products");
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

        insertRepository.Add(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);

        return created!.Adapt<OutputResponse>();
    }
}

