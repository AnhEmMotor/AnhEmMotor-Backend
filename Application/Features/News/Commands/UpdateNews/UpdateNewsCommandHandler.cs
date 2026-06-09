using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.News.Commands.UpdateNews;

public sealed class UpdateNewsCommandHandler(
    INewsReadRepository newsReadRepository,
    INewsUpdateRepository newsUpdateRepository,
    IUnitOfWork unitOfWork,
    IMemoryCache cache) : IRequestHandler<UpdateNewsCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
    {
        var news = await newsReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (news is null)
        {
            return Result<Unit>.Failure("Bài viết không tồn tại.");
        }
        
        var oldSlug = news.Slug;
        
        news.Title = request.Title;
        news.Slug = request.Slug ?? news.Slug;
        news.Content = request.Content;
        news.CoverImageUrl = request.CoverImageUrl;
        news.AuthorName = request.AuthorName;
        news.IsPublished = request.IsPublished;
        news.MetaTitle = request.MetaTitle;
        news.MetaDescription = request.MetaDescription;
        news.MetaKeywords = request.MetaKeywords;
        news.CategoryId = request.CategoryId;
        news.AuthorId = request.AuthorId;
        if (news.IsPublished && news.PublishedDate == null)
        {
            news.PublishedDate = DateTimeOffset.UtcNow;
        }

        if (request.LinkedProducts != null)
        {
            news.LinkedProducts.Clear();
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
                    news.LinkedProducts.Add(new Domain.Entities.NewsProduct
                    {
                        ProductVariantId = variantId,
                        ProductVariantColorId = colorId
                    });
                }
            }
        }

        newsUpdateRepository.Update(news);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        
        cache.Remove($"News_{news.Id}");
        if (!string.IsNullOrWhiteSpace(oldSlug))
        {
            cache.Remove($"News_Slug_{oldSlug}_Store");
        }
        if (!string.IsNullOrWhiteSpace(news.Slug) && news.Slug != oldSlug)
        {
            cache.Remove($"News_Slug_{news.Slug}_Store");
        }

        return Result<Unit>.Success(Unit.Value);
    }
}

