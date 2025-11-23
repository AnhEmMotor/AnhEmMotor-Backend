using Application.Interfaces.Repositories.Brand;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandReadRepository(ApplicationDBContext context) : IBrandReadRepository
{
    // --- MAIN PUBLIC METHODS ---

    public async Task<IEnumerable<BrandEntity>> GetAllAsync(CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        // Gọi hàm helper để lấy nguồn dữ liệu đúng
        var query = GetBaseQuery(mode);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<BrandEntity?> GetByIdAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await GetBaseQuery(mode)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<BrandEntity>> GetByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await GetBaseQuery(mode)
            .Where(b => ids.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }

    public IQueryable<BrandEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetBaseQuery(mode);
    }

    // --- PRIVATE HELPER METHOD ---
    // Đây là trái tim của Class này. Nó quyết định nguồn dữ liệu dựa trên Enum.
    // Giúp tuân thủ DRY (Don't Repeat Yourself)
    private IQueryable<BrandEntity> GetBaseQuery(DataFetchMode mode)
    {
        // Tận dụng các custom method có sẵn trong ApplicationDBContext của bạn
        return mode switch
        {
            DataFetchMode.ActiveOnly => context.Brands,                // Lấy chưa xóa
            DataFetchMode.DeletedOnly => context.DeletedOnly<BrandEntity>(), // Lấy đã xóa
            DataFetchMode.All => context.All<BrandEntity>(),           // Lấy tất cả
            _ => context.Brands // Fallback an toàn (mặc định là Active)
        };
    }
}