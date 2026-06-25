using Application.Common.Helper;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using Domain.Entities;
using MediatR;

namespace Application.Features.News.Commands.CreateNews;

public class CreateNewsCommandHandler(
    INewsInsertRepository newsInsertRepository,
    INewsReadRepository newsReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateNewsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Title)
            : SlugHelper.GenerateSlug(request.Slug);
        var existing = await newsReadRepository.GetBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
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
            MetaKeywords = request.MetaKeywords?.Trim(),
            CategoryId = request.CategoryId,
            AuthorId = request.AuthorId
        };
        if (request.LinkedProducts != null && request.LinkedProducts.Any())
        {
            foreach (var lp in request.LinkedProducts)
            {
                var parts = lp.Id.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[0], out var variantId))
                {
                    int? colorId = null;
                    if (int.TryParse(parts[1], out var parsedColorId))
                    {
                        colorId = parsedColorId;
                    }
                    news.LinkedProducts
                        .Add(new NewsProduct { ProductVariantId = variantId, ProductVariantColorId = colorId });
                }
            }
        }
        newsInsertRepository.Add(news);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(news.Id);
    }
}
