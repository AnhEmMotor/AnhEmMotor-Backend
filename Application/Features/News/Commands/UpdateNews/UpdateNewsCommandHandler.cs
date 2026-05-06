using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using MediatR;

namespace Application.Features.News.Commands.UpdateNews;

public sealed class UpdateNewsCommandHandler(
    INewsReadRepository newsReadRepository,
    INewsUpdateRepository newsUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateNewsCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
    {
        var news = await newsReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (news is null)
        {
            return Result<Unit>.Failure("Bài viết không tồn tại.");
        }
        news.Title = request.Title;
        news.Slug = request.Slug ?? news.Slug;
        news.Content = request.Content;
        news.CoverImageUrl = request.CoverImageUrl;
        news.AuthorName = request.AuthorName;
        news.IsPublished = request.IsPublished;
        news.MetaTitle = request.MetaTitle;
        news.MetaDescription = request.MetaDescription;
        news.MetaKeywords = request.MetaKeywords;
        if (news.IsPublished && news.PublishedDate == null)
        {
            news.PublishedDate = DateTimeOffset.UtcNow;
        }
        newsUpdateRepository.Update(news);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
