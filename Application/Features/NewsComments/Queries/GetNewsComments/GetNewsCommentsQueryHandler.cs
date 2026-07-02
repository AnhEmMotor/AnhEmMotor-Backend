using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.NewsComment;
using Application.Interfaces.Repositories.News;
using MediatR;

namespace Application.Features.NewsComments.Queries.GetNewsComments;

public class GetNewsCommentsQueryHandler(INewsCommentReadRepository newsCommentReadRepository, INewsReadRepository newsReadRepository)
    : IRequestHandler<GetNewsCommentsQuery, Result<List<NewsCommentResponse>>>
{
    public async Task<Result<List<NewsCommentResponse>>> Handle(
        GetNewsCommentsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.NewsComment> query = newsCommentReadRepository.GetQueryable();

        if (request.NewsId.HasValue)
            query = query.Where(c => c.NewsId == request.NewsId.Value);

        var comments = query
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var newsIds = comments.Select(c => c.NewsId).Distinct().ToList();
        var newsList = new List<Domain.Entities.News>();
        foreach (var id in newsIds)
        {
            var news = await newsReadRepository.GetByIdAsync(id, cancellationToken);
            if (news != null)
                newsList.Add(news);
        }

        var newsDict = newsList.ToDictionary(n => n.Id);

        var response = comments.Select(c => new NewsCommentResponse
        {
            Id = c.Id,
            NewsId = c.NewsId,
            NewsTitle = newsDict.TryGetValue(c.NewsId, out var n) ? n.Title : null,
            UserId = c.UserId,
            AuthorName = c.AuthorName,
            AuthorEmail = c.AuthorEmail,
            Content = c.Content,
            IsApproved = c.IsApproved,
            CreatedAt = c.CreatedAt ?? DateTimeOffset.UtcNow
        }).ToList();

        return Result<List<NewsCommentResponse>>.Success(response);
    }
}
