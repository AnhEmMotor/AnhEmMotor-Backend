using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreManyProductCategories;

public sealed class RestoreManyProductCategoriesCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyProductCategoriesCommand, (List<ProductCategoryResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<ProductCategoryResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
        RestoreManyProductCategoriesCommand request,
        CancellationToken cancellationToken)
    {
        if(request.Ids == null || request.Ids.Count == 0)
        {
            return ([], null);
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Common.Models.ErrorDetail>();

        var allCategories = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var deletedCategories = await readRepository.GetByIdAsync(
            uniqueIds,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var allCategoryMap = allCategories.ToDictionary(c => c.Id);
        var deletedCategorySet = deletedCategories.Select(c => c.Id).ToHashSet();

        foreach(var id in uniqueIds)
        {
            if(!allCategoryMap.ContainsKey(id))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Product category with Id {id} not found."
                    });
            } else if(!deletedCategorySet.Contains(id))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Product category with Id {id} is not deleted."
                    });
            }
        }

        if(errorDetails.Count > 0)
        {
            return (null, new Common.Models.ErrorResponse { Errors = errorDetails });
        }

        if(deletedCategories.ToList().Count > 0)
        {
            updateRepository.Restore(deletedCategories);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (deletedCategories.Adapt<List<ProductCategoryResponse>>(), null);
    }
}
