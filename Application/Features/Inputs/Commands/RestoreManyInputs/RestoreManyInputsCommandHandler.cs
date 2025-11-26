using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Enums;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed class RestoreManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyInputsCommand, Unit>
{
    public async Task<Unit> Handle(
        RestoreManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var inputs = await readRepository.GetByIdAsync(
            request.Ids,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if (inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            throw new InvalidOperationException(
                $"Không tìm thấy {missingIds.Count} phiếu nhập đã xóa: {string.Join(", ", missingIds)}");
        }

        updateRepository.Restore(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
