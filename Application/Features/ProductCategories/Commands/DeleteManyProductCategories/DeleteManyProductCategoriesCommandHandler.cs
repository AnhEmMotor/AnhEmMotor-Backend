using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteManyProductCategories;

public sealed class DeleteManyProductCategoriesCommandHandler(IProductCategorySelectRepository selectRepository, IProductCategoryDeleteRepository deleteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteManyProductCategoriesCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteManyProductCategoriesCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<ErrorDetail>();

        var allCategories = await selectRepository.GetAllCategoriesByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var activeCategories = await selectRepository.GetActiveCategoriesByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allCategoryMap = allCategories.ToDictionary(c => c.Id);
        var activeCategorySet = activeCategories.Select(c => c.Id).ToHashSet();

        foreach (var id in uniqueIds)
        {
            if (!allCategoryMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Product category with Id {id} not found."
                });
            }
            else if (!activeCategorySet.Contains(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Product category with Id {id} has already been deleted."
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (activeCategories.Count > 0)
        {
            deleteRepository.Delete(activeCategories);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}
