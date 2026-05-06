using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using MediatR;

namespace Application.Features.News.Commands.DeleteNews;

public sealed class DeleteNewsCommandHandler(
    INewsReadRepository newsReadRepository,
    INewsDeleteRepository newsDeleteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteNewsCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
    {
        var news = await newsReadRepository.GetByIdAsync(request.Id, cancellationToken);

        if (news is null)
        {
            return Result<Unit>.Failure("Bài viết không tồn tại.");
        }

        newsDeleteRepository.Delete(news);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
