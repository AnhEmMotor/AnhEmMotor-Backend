using Application.Interfaces.Repositories.NewsComment;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.NewsComment;

public class NewsCommentInsertRepository(ApplicationDBContext context) : INewsCommentInsertRepository
{
    public void Add(Domain.Entities.NewsComment comment)
    {
        context.NewsComments.Add(comment);
    }

    public void Update(Domain.Entities.NewsComment comment)
    {
        context.NewsComments.Update(comment);
    }

    public void Remove(Domain.Entities.NewsComment comment)
    {
        context.NewsComments.Remove(comment);
    }
}
