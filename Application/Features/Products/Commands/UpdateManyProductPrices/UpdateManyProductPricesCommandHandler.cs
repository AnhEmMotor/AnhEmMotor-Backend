using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed class UpdateManyProductPricesCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository, // Giữ lại nếu cần logic update đặc thù
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyProductPricesCommand, (List<int>? Data, ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, ErrorResponse? Error)> Handle(
        UpdateManyProductPricesCommand command,
        CancellationToken cancellationToken)
    {
        var productIds = command.Ids.Distinct().ToList();

        // 1. Fetch Data (Repository đã abstract DB)
        // Mock: Setup(r => r.GetByIdsWithVariantsAsync(...)).ReturnsAsync(fakeList);
        var products = await readRepository.GetByIdWithVariantsAsync(productIds, cancellationToken);
        var productList = products.ToList(); // Materialize để xử lý

        // 2. Validation Logic (Pure C# - Dễ test)
        if (productList.Count != productIds.Count)
        {
            var foundIds = productList.Select(p => p.Id).ToHashSet();
            var missingErrors = productIds
                .Where(id => !foundIds.Contains(id))
                .Select(id => new ErrorDetail
                {
                    Field = id.ToString(),
                    Message = $"Sản phẩm với Id {id} không tồn tại."
                })
                .ToList();

            return (null, new ErrorResponse { Errors = missingErrors });
        }

        // 3. Business Logic (Pure C# - Update in Memory)
        foreach (var product in productList)
        {
            if (product.ProductVariants != null)
            {
                foreach (var variant in product.ProductVariants)
                {
                    variant.Price = command.Price;
                }
            }

            // Nếu hàm Update của bạn chỉ là context.Update(entity), thì dòng này có thể bỏ qua 
            // nếu entity đã được track. Nhưng để an toàn theo pattern của bạn, cứ giữ lại.
            updateRepository.Update(product);
        }

        // 4. Persist (UnitOfWork abstract DB transaction)
        // Mock: Setup(u => u.SaveChangesAsync(...)).ReturnsAsync(1);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (productIds, null);
    }
}