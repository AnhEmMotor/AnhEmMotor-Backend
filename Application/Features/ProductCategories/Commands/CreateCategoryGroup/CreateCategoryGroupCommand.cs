using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.ProductCategories.Commands.CreateCategoryGroup;

public sealed record CreateCategoryGroupCommand : IRequest<Result<string>>
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("category_ids")]
    public List<int> CategoryIds { get; init; } = [];
}

public sealed class CreateCategoryGroupCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryGroupCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateCategoryGroupCommand request, CancellationToken cancellationToken)
    {
        var categories = await readRepository.GetByIdAsync(request.CategoryIds, cancellationToken).ConfigureAwait(false);
        
        if (categories == null || categories.Count == 0)
        {
            return Result<string>.Failure(Error.NotFound("Không tìm thấy danh mục nào."));
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
