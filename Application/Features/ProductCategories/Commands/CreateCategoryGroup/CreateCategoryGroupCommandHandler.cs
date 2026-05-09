using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using MediatR;

namespace Application.Features.ProductCategories.Commands.CreateCategoryGroup;

public sealed class CreateCategoryGroupCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryGroupCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateCategoryGroupCommand request, CancellationToken cancellationToken)
    {
        var categories = (await readRepository.GetByIdAsync(request.CategoryIds, cancellationToken).ConfigureAwait(false)).ToList();
        
        if (categories.Count == 0)
        {
            return Result<string>.Failure(new Error("CategoryGroup.NotFound", "Không tìm thấy danh mục nào."));
        }

        foreach (var category in categories)
        {
            category.CategoryGroup = request.Name;
            updateRepository.Update(category);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<string>.Success(request.Name);
    }
}
