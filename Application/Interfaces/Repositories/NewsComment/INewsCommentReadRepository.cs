
namespace Application.Interfaces.Repositories.NewsComment;

public interface INewsCommentReadRepository
{
    public IQueryable<Domain.Entities.NewsComment> GetQueryable();

    public Task<List<Domain.Entities.NewsComment>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<Domain.Entities.NewsComment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.NewsComment>> GetByNewsIdAsync(int newsId, CancellationToken cancellationToken = default);
}
