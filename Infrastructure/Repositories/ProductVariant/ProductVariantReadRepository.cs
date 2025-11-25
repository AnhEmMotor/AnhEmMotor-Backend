using Application.Interfaces.Repositories.ProductVariant;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVariantReadRepository(ApplicationDBContext context) : IProductVariantReadRepository
    {
        public IQueryable<ProductVariantEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<ProductVariantEntity>(mode);
        }

        public async Task<ProductVariantEntity?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return await context.GetQuery<ProductVariantEntity>(mode)
                .Include(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }

        public async Task<ProductVariantEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return await context.GetQuery<ProductVariantEntity>(mode)
                .FirstOrDefaultAsync(v => v.UrlSlug == slug, cancellationToken);
        }

        public async Task<IEnumerable<ProductVariantEntity>> GetByProductIdAsync(int productId, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return await context.GetQuery<ProductVariantEntity>(mode)
                .Where(v => v.ProductId == productId)
                // Include Product để lấy tên Product
                .Include(v => v.Product)
                // Include InputInfos để tính Stock
                .Include(v => v.InputInfos)
                // Include Options để hiển thị cặp thuộc tính (Màu: Đỏ, Size: L)
                .Include(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductVariantEntity>> GetByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return await context.GetQuery<ProductVariantEntity>(mode)
                .Where(v => ids.Contains(v.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<ProductVariantEntity> Items, int TotalCount)> GetPagedVariantsAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            // 1. Base Query theo Mode (Đã xử lý DeletedAt của chính Variant)
            var query = context.GetQuery<ProductVariantEntity>(mode);

            // 2. Filter quan hệ với Product cha
            if (mode == DataFetchMode.ActiveOnly)
            {
                // Active Variants thuộc Active Products
                query = query.Where(v => v.Product != null && v.Product.DeletedAt == null);
            }
            else if (mode == DataFetchMode.DeletedOnly)
            {
                // Logic của bạn: Deleted Variants thuộc Active Products
                // (Nếu bạn muốn lấy cả Variant xóa của Product xóa, bỏ điều kiện DeletedAt == null)
                query = query.Where(v => v.Product != null && v.Product.DeletedAt == null);
            }

            // 3. Count
            var totalCount = await query.CountAsync(cancellationToken);

            // 4. Fetch Data (Include logic gom gọn lại)
            var items = await query
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Brand)
                .Include(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
                .Include(v => v.InputInfos)
                .Include(v => v.OutputInfos)
                    .ThenInclude(oi => oi.OutputOrder)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
