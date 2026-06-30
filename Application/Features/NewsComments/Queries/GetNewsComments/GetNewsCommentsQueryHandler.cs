using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.NewsComment;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var newsIds = comments.Select(c => c.NewsId).Distinct().ToList();
        var newsList = await newsReadRepository.GetQueryable()
            .Where(n => newsIds.Contains(n.Id))
            .ToListAsync(cancellationToken);

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
            CreatedAt = c.CreatedAt
        }).ToList();

        return Result<List<NewsCommentResponse>>.Success(response);
    }
}
