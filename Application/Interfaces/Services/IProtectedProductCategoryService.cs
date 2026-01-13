namespace Application.Interfaces.Services;

/// <summary>
/// Service quản lý logic bảo vệ danh mục sản phẩm
/// </summary>
public interface IProtectedProductCategoryService
{
    /// <summary>
    /// Kiểm tra xem danh mục sản phẩm có được bảo vệ không (theo ID)
    /// </summary>
    /// <param name="categoryId">ID của danh mục sản phẩm</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True nếu danh mục được bảo vệ, false nếu không</returns>
    public Task<bool> IsProtectedAsync(int categoryId, CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra xem danh mục sản phẩm có được bảo vệ không (theo tên)
    /// </summary>
    /// <param name="categoryName">Tên danh mục sản phẩm</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True nếu danh mục được bảo vệ, false nếu không</returns>
    public Task<bool> IsProtectedByNameAsync(string categoryName, CancellationToken cancellationToken);
}
