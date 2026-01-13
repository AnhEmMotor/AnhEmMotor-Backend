using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;

using Domain.Constants;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteManyProductCategories;

public sealed class DeleteManyProductCategoriesCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyProductCategoriesCommand, Result>
{
    public async Task<Result> Handle(DeleteManyProductCategoriesCommand request, CancellationToken cancellationToken)
    {
        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Error>();

        var allCategories = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var activeCategories = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allCategoryMap = allCategories.ToDictionary(c => c.Id);
        var activeCategorySet = activeCategories.Select(c => c.Id).ToHashSet();

        foreach(var id in uniqueIds)
        {
            if(!allCategoryMap.ContainsKey(id))
            {
                errorDetails.Add(Error.NotFound($"Product category with Id {id} not found.", "Id"));
            } else if(!activeCategorySet.Contains(id))
            {
                errorDetails.Add(Error.BadRequest($"Product category with Id {id} has already been deleted.", "Id"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        if(activeCategories.ToList().Count > 0)
        {
            deleteRepository.Delete(activeCategories);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}
