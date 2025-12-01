using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed class DeleteManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyInputsCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(
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
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Ids", Message = $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}" } ]
            };
        }

        deleteRepository.Delete(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
