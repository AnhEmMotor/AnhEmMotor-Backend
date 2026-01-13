namespace Application.Interfaces.Services;

public interface IProtectedProductCategoryService
{
    public Task<bool> IsProtectedAsync(int categoryId, CancellationToken cancellationToken);

    public Task<bool> IsProtectedByNameAsync(string categoryName, CancellationToken cancellationToken);
}
