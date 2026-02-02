using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using Domain.Constants;
using MediatR;

namespace Application.Features.Products.Commands.DeleteManyProducts;

public sealed class DeleteManyProductsCommandHandler(
    IProductReadRepository readRepository,
    IProductDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyProductsCommand, Result>
{
    public async Task<Result> Handle(DeleteManyProductsCommand command, CancellationToken cancellationToken)
    {
        var uniqueIds = command.Ids!.Distinct().ToList();

        // CHỈ GỌI DB 1 LẦN: Lấy tất cả bao gồm cả những sản phẩm đã bị xóa (Soft Deleted)
        // Giả sử DataFetchMode.All bao gồm cả deleted, nếu không hãy điều chỉnh Repo
        var allProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id!);
        var errorDetails = new List<Error>();

        foreach (var id in uniqueIds)
        {
            // Bước 1: Kiểm tra tồn tại
            if (!allProductsMap.TryGetValue(id, out var product))
            {
                errorDetails.Add(Error.NotFound($"Product not found, Product ID: {id}"));
                continue; // Bỏ qua các kiểm tra sau cho ID này
            }

            // Bước 2: Kiểm tra trạng thái (Ví dụ: dựa trên IsDeleted hoặc StatusId)
            // Giả sử logic của bạn là check xem nó có còn "Active" không
            if (product.DeletedAt != null) // Hoặc logic IsActive của bạn
            {
                errorDetails.Add(Error.BadRequest(
                    $"Product has already been deleted, Product ID: {id}, Product Name: {product.Name}"));
            }
        }

        if (errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        // Lọc ra danh sách thực sự cần xóa (những thằng đang active)
        var productsToDelete = allProductsMap.Values.Where(p => p.DeletedAt == null).ToList();

        if (productsToDelete.Count > 0)
        {
            deleteRepository.Delete(productsToDelete);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}
