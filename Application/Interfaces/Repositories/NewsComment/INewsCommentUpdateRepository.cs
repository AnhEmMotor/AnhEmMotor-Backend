using Domain.Entities;

namespace Application.Interfaces.Repositories.NewsComment;

public interface INewsCommentUpdateRepository
{
    void Update(Domain.Entities.NewsComment comment);
}
