using Application.Interfaces.Repositories.Product;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductReadRepository(ApplicationDBContext context) : IProductReadRepository
{
    public IQueryable<ProductEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    { return context.GetQuery<ProductEntity>(mode).Include(p => p.ProductCategory).Include(p => p.Brand); }

    public Task<IEnumerable<ProductEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<ProductEntity>>(t => t.Result, cancellationToken);
    }

    public Task<ProductEntity?> GetByIdAsync(
    int id,
    CancellationToken cancellationToken,
    DataFetchMode mode = DataFetchMode.ActiveOnly) 
    {
        IQueryable<ProductEntity> query = context.Products.IgnoreQueryFilters();

        // 2. Tự lọc Product theo mode
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(p => p.DeletedAt == null);
        }
        else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(p => p.DeletedAt != null);
        }

        // 3. Include đầy đủ và lọc Variant rác
        return query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            // Lấy Variant nhưng lọc bỏ những thằng đã xóa (nếu đang xem Active)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<ProductEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<ProductEntity>>(t => t.Result, cancellationToken);
    }

    public Task<ProductEntity?> GetByIdWithDetailsAsync(
    int id,
    CancellationToken cancellationToken,
    DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        IQueryable<ProductEntity> query = context.Products.IgnoreQueryFilters();

        // Xử lý logic mode cho Product cha
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(p => p.DeletedAt == null);
        }
        else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(p => p.DeletedAt != null);
        }

        // Với ProductVariants, logic thường là:
        // - Nếu xem Product Active -> Chỉ xem Variant Active.
        // - Nếu xem Product đã xóa -> Có thể muốn xem tất cả Variant cũ.
        // Dưới đây tôi giả định: Xem Product kiểu gì thì Variant cũng phải theo mode đó hoặc Active.
        // Tuy nhiên, an toàn nhất cho UI hiển thị là chỉ lấy Active Variants trừ khi bạn cố tình muốn debug.

        // Ở đây tôi áp dụng: Luôn chỉ lấy Active Variants để tránh rác, 
        // trừ khi bạn muốn view lịch sử sâu (lúc đó cần logic phức tạp hơn).

        return query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null)) // <--- CHỈ SỬA CHỖ NÀY
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
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<ProductEntity>> GetByIdWithVariantsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<ProductEntity>(mode)
            .Where(p => ids.Contains(p.Id))
            .Include(p => p.ProductVariants)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<ProductEntity>>(t => t.Result, cancellationToken);
    }

    public async Task<(List<ProductEntity> Items, int TotalCount)> GetPagedDeletedProductsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.DeletedOnly<ProductEntity>();

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
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
            .OrderByDescending(p => p.DeletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

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

        // 1. Tắt filter global để lấy Brand/Category đã xóa
        // 2. Tự lọc Product Active bằng tay
        var query = context.Products
            .IgnoreQueryFilters()
            .Where(p => p.DeletedAt == null)
            .AsNoTracking();

        if (searchPattern != null)
        {
            query = query.Where(
                p => EF.Functions.Like(p.Name, searchPattern) ||
                     (p.ProductCategory != null && EF.Functions.Like(p.ProductCategory.Name, searchPattern)) ||
                     (p.Brand != null && EF.Functions.Like(p.Brand.Name, searchPattern)));
        }

        if (statusIds != null && statusIds.Count > 0)
        {
            query = query.Where(p => p.StatusId != null && statusIds.Contains(p.StatusId));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var entities = await query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            // Lấy ProductVariant chưa xóa
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
                // FIX: Chỉ lấy InputInfo chưa xóa.
                // Vì IgnoreQueryFilters() ở trên cùng đã tắt bộ lọc của bảng này rồi.
                .ThenInclude(v => v.InputInfos.Where(ii => ii.DeletedAt == null && ii.InputReceipt!.DeletedAt == null))

            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
                // FIX: Tương tự với OutputInfo, phải lọc cái chưa xóa thì tồn kho mới đúng
                .ThenInclude(v => v.OutputInfos.Where(oi => oi.DeletedAt == null && oi.OutputOrder!.DeletedAt == null))
                .ThenInclude(oi => oi.OutputOrder) // Giả sử OutputOrder cũng cần check deleted thì thêm Where ở đây luôn

            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
                .ThenInclude(v => v.ProductCollectionPhotos)

            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
                .ThenInclude(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                .ThenInclude(ov => ov!.Option)

            .OrderByDescending(p => p.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (entities, totalCount);
    }
}