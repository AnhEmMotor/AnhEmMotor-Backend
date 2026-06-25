using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.News.Commands.UpdateNewsStatus;

public class UpdateNewsStatusCommandHandler(
    INewsReadRepository newsReadRepository,
    INewsUpdateRepository newsUpdateRepository,
    IUnitOfWork unitOfWork,
    IMemoryCache cache) : IRequestHandler<UpdateNewsStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateNewsStatusCommand request, CancellationToken cancellationToken)
    {
        var news = await newsReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (news is null)
        {
            return Result<Unit>.Failure("Bài viết không tồn tại.");
        }
        news.IsPublished = request.IsPublished;
        if (news.IsPublished && news.PublishedDate == null)
        {
            news.PublishedDate = DateTimeOffset.UtcNow;
        }
        newsUpdateRepository.Update(news);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        cache.Remove($"News_{news.Id}");
        if (!string.IsNullOrWhiteSpace(news.Slug))
        {
            cache.Remove($"News_Slug_{news.Slug}_Store");
        }
        return Result<Unit>.Success(Unit.Value);
    }
}
