using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed class DeleteManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyInputsCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var inputs = await readRepository.GetByIdAsync(
            request.Ids,
            cancellationToken)
            .ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if (inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            throw new InvalidOperationException(
                $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}");
        }

        deleteRepository.Delete(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
