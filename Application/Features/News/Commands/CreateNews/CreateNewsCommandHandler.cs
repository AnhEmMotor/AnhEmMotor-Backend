using Application.Common.Helper;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using MediatR;

namespace Application.Features.News.Commands.CreateNews;

public sealed class CreateNewsCommandHandler(
    INewsInsertRepository newsInsertRepository,
    INewsReadRepository newsReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateNewsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug) 
            ? SlugHelper.GenerateSlug(request.Title) 
            : SlugHelper.GenerateSlug(request.Slug);

        var existing = await newsReadRepository.GetBySlugAsync(slug, cancellationToken);
        if (existing != null)
        {
            return Result<int>.Failure(Error.BadRequest($"Đường dẫn (slug) '{slug}' đã tồn tại.", nameof(request.Slug)));
        }

        var news = new Domain.Entities.News
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Content = request.Content,
            CoverImageUrl = request.CoverImageUrl,
            AuthorName = request.AuthorName,
            IsPublished = request.IsPublished,
            PublishedDate = request.IsPublished ? DateTimeOffset.UtcNow : null,
            MetaTitle = request.MetaTitle?.Trim(),
            MetaDescription = request.MetaDescription?.Trim(),
            MetaKeywords = request.MetaKeywords?.Trim()
        };

        newsInsertRepository.Add(news);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(news.Id);
    }
}
