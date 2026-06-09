using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.News.Commands.DeleteNews;

public sealed class DeleteNewsCommandHandler(
    INewsReadRepository newsReadRepository,
    INewsDeleteRepository newsDeleteRepository,
    IUnitOfWork unitOfWork,
    IMemoryCache cache) : IRequestHandler<DeleteNewsCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
    {
        var news = await newsReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (news is null)
        {
            return Result<Unit>.Failure("Bài viết không tồn tại.");
        }
        newsDeleteRepository.Delete(news);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        cache.Remove($"News_{news.Id}");
        if (!string.IsNullOrWhiteSpace(news.Slug))
        {
            cache.Remove($"News_Slug_{news.Slug}_Store");
        }
        return Result<Unit>.Success(Unit.Value);
    }
}
