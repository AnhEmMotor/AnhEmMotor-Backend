using Domain.Entities;

namespace Application.Interfaces.Repositories.NewsComment;

public interface INewsCommentInsertRepository
{
    void Add(Domain.Entities.NewsComment comment);
    void Update(Domain.Entities.NewsComment comment);
    void Remove(Domain.Entities.NewsComment comment);
}
