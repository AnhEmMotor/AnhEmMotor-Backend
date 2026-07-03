using Application.Interfaces.Repositories.NewsComment;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.NewsComment;

public class NewsCommentReadRepository(ApplicationDBContext context) : INewsCommentReadRepository
{
    public IQueryable<Domain.Entities.NewsComment> GetQueryable() => context.Set<Domain.Entities.NewsComment>().AsQueryable();

    public Task<List<Domain.Entities.NewsComment>> GetAllAsync(CancellationToken cancellationToken = default)
        => context.Set<Domain.Entities.NewsComment>()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Domain.Entities.NewsComment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Set<Domain.Entities.NewsComment>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<Domain.Entities.NewsComment>> GetByNewsIdAsync(int newsId, CancellationToken cancellationToken = default)
        => context.Set<Domain.Entities.NewsComment>()
            .Where(c => c.NewsId == newsId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
}
