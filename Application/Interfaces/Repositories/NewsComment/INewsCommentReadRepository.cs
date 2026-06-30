using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces.Repositories.NewsComment;

public interface INewsCommentReadRepository
{
    IQueryable<NewsComment> GetQueryable();
    Task<List<NewsComment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NewsComment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<NewsComment>> GetByNewsIdAsync(int newsId, CancellationToken cancellationToken = default);
}
