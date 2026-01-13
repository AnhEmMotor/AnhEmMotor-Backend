using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreManyProductCategories;

public sealed class RestoreManyProductCategoriesCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyProductCategoriesCommand, Result<List<ProductCategoryResponse>?>>
{
    public async Task<Result<List<ProductCategoryResponse>?>> Handle(
        RestoreManyProductCategoriesCommand request,
        CancellationToken cancellationToken)
    {
        if(request.Ids == null || request.Ids.Count == 0)
        {
            return Error.BadRequest("You not pass Ids to restore.");
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Error>();

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
                errorDetails.Add(Error.NotFound($"Product category with Id {id} not found.", "Id"));
            } else if(!deletedCategorySet.Contains(id))
            {
                errorDetails.Add(Error.BadRequest($"Product category with Id {id} is not deleted.", "Id"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return errorDetails;
        }

        if(deletedCategories.ToList().Count > 0)
        {
            updateRepository.Restore(deletedCategories);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return deletedCategories.Adapt<List<ProductCategoryResponse>>();
    }
}
