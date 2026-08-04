using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using Application.Interfaces.Repositories.NewsComment;
using Domain.Entities;
using MediatR;

namespace Application.Features.NewsComments.Commands.CreateNewsComment;

public class CreateNewsCommentCommandHandler(
    INewsCommentInsertRepository newsCommentInsertRepository,
    INewsReadRepository newsReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateNewsCommentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateNewsCommentCommand request, CancellationToken cancellationToken)
    {
        int? resolvedNewsId = request.NewsId;
        if (!resolvedNewsId.HasValue && request.ArticleType == "news" && !string.IsNullOrEmpty(request.ArticleSlug))
        {
            var news = await newsReadRepository.GetBySlugAsync(request.ArticleSlug, cancellationToken);
            if (news != null)
            {
                resolvedNewsId = news.Id;
            }
        }
        var comment = new NewsComment
        {
            NewsId = resolvedNewsId,
            ArticleType = request.ArticleType,
            ArticleSlug = request.ArticleSlug,
            UserId = request.UserId,
            AuthorName = request.AuthorName,
            AuthorEmail = request.AuthorEmail,
            Content = request.Content,
            IsApproved = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        newsCommentInsertRepository.Add(comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(comment.Id);
    }
}
