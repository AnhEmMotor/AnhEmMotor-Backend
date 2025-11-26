using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Enums;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed class RestoreManyOutputsCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyOutputsCommand, Unit>
{
    public async Task<Unit> Handle(
        RestoreManyOutputsCommand request,
        CancellationToken cancellationToken)
    {
        var outputs = await readRepository.GetByIdAsync(
            request.Ids,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var outputsList = outputs.ToList();

        if (outputsList.Count != request.Ids.Count)
        {
            var foundIds = outputsList.Select(o => o.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            throw new InvalidOperationException(
                $"Không tìm thấy {missingIds.Count} đơn hàng đã xóa: {string.Join(", ", missingIds)}");
        }

        updateRepository.Restore(outputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
