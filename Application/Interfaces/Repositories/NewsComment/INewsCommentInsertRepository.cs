
namespace Application.Interfaces.Repositories.NewsComment;

public interface INewsCommentInsertRepository
{
    public void Add(Domain.Entities.NewsComment comment);

    public void Update(Domain.Entities.NewsComment comment);

    public void Remove(Domain.Entities.NewsComment comment);
}
