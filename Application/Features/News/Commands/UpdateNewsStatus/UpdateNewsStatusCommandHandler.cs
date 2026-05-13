using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using MediatR;

namespace Application.Features.News.Commands.UpdateNewsStatus;

public sealed class UpdateNewsStatusCommandHandler(
    INewsReadRepository newsReadRepository,
    INewsUpdateRepository newsUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateNewsStatusCommand, Result<Unit>>
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
        return Result<Unit>.Success(Unit.Value);
    }
}
