using Domain.Entities;

namespace Application.Interfaces.Repositories.NewsComment;

public interface INewsCommentReadRepository
{
    IQueryable<Domain.Entities.NewsComment> GetQueryable();
    Task<List<Domain.Entities.NewsComment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.NewsComment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.NewsComment>> GetByNewsIdAsync(int newsId, CancellationToken cancellationToken = default);
}
