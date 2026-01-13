using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

/// <summary>
/// Implementation service để kiểm tra danh mục sản phẩm có được bảo vệ không
/// </summary>
public class ProtectedProductCategoryService(
    IProductCategoryReadRepository readRepository,
    IConfiguration configuration) : IProtectedProductCategoryService
{
    private List<string>? _cachedProtectedCategories;

    /// <inheritdoc/>
    public async Task<bool> IsProtectedAsync(int categoryId, CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(categoryId, cancellationToken).ConfigureAwait(false);

        if (category is null || string.IsNullOrWhiteSpace(category.Name))
        {
            return false;
        }

        return await IsProtectedByNameAsync(category.Name, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<bool> IsProtectedByNameAsync(string categoryName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return Task.FromResult(false);
        }

        _cachedProtectedCategories ??= configuration.GetSection("ProtectedProductCategory").Get<List<string>>() ?? [];

        var isProtected = _cachedProtectedCategories
            .Any(pc => string.Equals(pc, categoryName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(isProtected);
    }
}
