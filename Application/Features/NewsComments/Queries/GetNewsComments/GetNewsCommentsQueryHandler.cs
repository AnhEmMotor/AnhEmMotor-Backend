using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using Application.Interfaces.Repositories.NewsComment;
using Domain.Entities;
using MediatR;

namespace Application.Features.NewsComments.Queries.GetNewsComments;

public class GetNewsCommentsQueryHandler(
    INewsCommentReadRepository newsCommentReadRepository,
    INewsReadRepository newsReadRepository) : IRequestHandler<GetNewsCommentsQuery, Result<List<NewsCommentResponse>>>
{
    public async Task<Result<List<NewsCommentResponse>>> Handle(
        GetNewsCommentsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<NewsComment> query = newsCommentReadRepository.GetQueryable();
        if (request.NewsId.HasValue)
            query = query.Where(c => c.NewsId == request.NewsId.Value);
        if (!string.IsNullOrEmpty(request.ArticleType))
            query = query.Where(c => c.ArticleType == request.ArticleType);
        if (!string.IsNullOrEmpty(request.ArticleSlug))
            query = query.Where(c => c.ArticleSlug == request.ArticleSlug);

        var comments = query
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
            
        var newsIds = comments.Where(c => c.NewsId.HasValue).Select(c => c.NewsId!.Value).Distinct().ToList();
        var newsSlugs = comments.Where(c => !c.NewsId.HasValue && c.ArticleType == "news" && !string.IsNullOrEmpty(c.ArticleSlug))
                                .Select(c => c.ArticleSlug!).Distinct().ToList();
        
        var newsList = new List<Domain.Entities.News>();
        foreach (var id in newsIds)
        {
            var news = await newsReadRepository.GetByIdAsync(id, cancellationToken);
            if (news != null)
                newsList.Add(news);
        }
        foreach (var slug in newsSlugs)
        {
            var news = await newsReadRepository.GetBySlugAsync(slug, cancellationToken);
            if (news != null)
                newsList.Add(news);
        }
        
        var newsDictById = newsList.GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
        var newsDictBySlug = newsList.Where(n => !string.IsNullOrEmpty(n.Slug)).GroupBy(n => n.Slug).ToDictionary(g => g.Key, g => g.First());
        
        string? GetTitle(NewsComment c) {
            if (c.NewsId.HasValue && newsDictById.TryGetValue(c.NewsId.Value, out var n)) return n.Title;
            if (!c.NewsId.HasValue && c.ArticleType == "news" && !string.IsNullOrEmpty(c.ArticleSlug) && newsDictBySlug.TryGetValue(c.ArticleSlug, out var ns)) return ns.Title;
            return null;
        }

        string? GetImage(NewsComment c) {
            if (c.NewsId.HasValue && newsDictById.TryGetValue(c.NewsId.Value, out var n)) return n.CoverImageUrl;
            if (!c.NewsId.HasValue && c.ArticleType == "news" && !string.IsNullOrEmpty(c.ArticleSlug) && newsDictBySlug.TryGetValue(c.ArticleSlug, out var ns)) return ns.CoverImageUrl;
            return null;
        }
        
        var response = comments.Select(
            c => new NewsCommentResponse
            {
                Id = c.Id,
                NewsId = c.NewsId,
                NewsTitle = GetTitle(c),
                NewsImage = GetImage(c),
                ArticleType = c.ArticleType,
                ArticleSlug = c.ArticleSlug,
                UserId = c.UserId,
                AuthorName = c.AuthorName,
                AuthorEmail = c.AuthorEmail,
                Content = c.Content,
                IsApproved = c.IsApproved,
                CreatedAt = c.CreatedAt ?? DateTimeOffset.UtcNow
            })
            .ToList();
        return Result<List<NewsCommentResponse>>.Success(response);
    }
}
