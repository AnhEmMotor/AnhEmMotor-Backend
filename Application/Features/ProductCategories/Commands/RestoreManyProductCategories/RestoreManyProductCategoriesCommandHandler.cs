using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Enums;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreManyProductCategories;

public sealed class RestoreManyProductCategoriesCommandHandler(IProductCategoryReadRepository readRepository, IProductCategoryUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreManyProductCategoriesCommand, (List<ProductCategoryResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<ProductCategoryResponse>? Data, ErrorResponse? Error)> Handle(RestoreManyProductCategoriesCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return ([], null);
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<ErrorDetail>();

        var allCategories = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All).ConfigureAwait(false);
        var deletedCategories = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.DeletedOnly).ConfigureAwait(false);

        var allCategoryMap = allCategories.ToDictionary(c => c.Id);
        var deletedCategorySet = deletedCategories.Select(c => c.Id).ToHashSet();

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
            else if (!deletedCategorySet.Contains(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Product category with Id {id} is not deleted."
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        if (deletedCategories.ToList().Count > 0)
        {
            updateRepository.Restore(deletedCategories);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (deletedCategories.Adapt<List<ProductCategoryResponse>>(), null);
    }
}
