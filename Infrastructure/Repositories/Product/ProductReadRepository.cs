using Application.Interfaces.Repositories.Product;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductReadRepository(ApplicationDBContext context) : IProductReadRepository
{
    public IQueryable<ProductEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<ProductEntity>(mode)
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand);
    }

    public async Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await GetQueryable(mode)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.All)
    {
        return await GetQueryable(mode)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ProductEntity>> GetByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await GetQueryable(mode)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductEntity?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<ProductEntity>(mode)
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.OutputInfos)
                    .ThenInclude(oi => oi.OutputOrder)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ProductEntity>> GetByIdWithVariantsAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<ProductEntity>(mode)
            .Where(p => ids.Contains(p.Id))
            .Include(p => p.ProductVariants) // Nhu cầu của bạn nằm ở đây
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ProductEntity> Items, int TotalCount)> GetPagedDeletedProductsAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken)
    {
        // 1. Base Query: Lấy những Product đã xóa (DeletedOnly)
        var query = context.DeletedOnly<ProductEntity>();

        // 2. Đếm tổng
        var totalCount = await query.CountAsync(cancellationToken);

        // 3. Fetch Data với Include đầy đủ (Logic copy từ Handler cũ của bạn nhưng chuẩn hóa)
        // Lưu ý: Với Product đã xóa, ta cũng muốn lấy Variants đã xóa tương ứng (hoặc tất cả variants thuộc nó).
        // Ở đây ta dùng IgnoreQueryFilters() gián tiếp thông qua việc Include mà không filter DeletedAt null
        // TUY NHIÊN: Logic cũ của bạn đang filter variant deletedAt == null (tức là lấy variant CHƯA XÓA của product ĐÃ XÓA).
        // Điều này hơi lạ về nghiệp vụ (Product xóa thì Variant thường cũng xóa), nhưng tôi tôn trọng logic cũ của bạn.

        /* Logic cũ của bạn: 
           .Include(p => p.ProductVariants.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
           Nghĩa là: Lấy Product ĐÃ xóa, kèm theo Variant CHƯA xóa.
        */

        // Để code Clean, ta không dùng magic string "AuditingProperties.DeletedAt".
        // Ta dùng biểu thức lambda trực tiếp nếu Variant kế thừa BaseEntity/AuditableEntity

        var items = await query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            // Lấy Variant chưa xóa (DeletedAt == null) của Product đã xóa
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
                .ThenInclude(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
                .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
                .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
                .ThenInclude(v => v.OutputInfos)
                    .ThenInclude(oi => oi.OutputOrder)
            .OrderByDescending(p => p.DeletedAt) // Nên sort theo thời gian xóa
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<ProductEntity> Items, int TotalCount)> GetPagedProductsAsync(
        string? search,
        List<string> statusIds,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);
        var searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";

        // 1. Base Query
        var query = context.GetQuery<ProductEntity>(DataFetchMode.ActiveOnly)
            .AsNoTracking();

        // 2. Filter (Search Pattern)
        if (searchPattern != null)
        {
            query = query.Where(p =>
                EF.Functions.Like(p.Name, searchPattern) ||
                (p.ProductCategory != null && EF.Functions.Like(p.ProductCategory.Name, searchPattern)) ||
                (p.Brand != null && EF.Functions.Like(p.Brand.Name, searchPattern))
            );
        }

        // 3. Filter (Status)
        if (statusIds != null && statusIds.Count > 0)
        {
            query = query.Where(p => p.StatusId != null && statusIds.Contains(p.StatusId));
        }

        // 4. Count Total
        var totalCount = await query.CountAsync(cancellationToken);

        // 5. Fetch Data
        var entities = await query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.OutputInfos)
                    .ThenInclude(oi => oi.OutputOrder)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return (entities, totalCount);
    }
}