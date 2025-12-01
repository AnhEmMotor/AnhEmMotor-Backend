using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Enums;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed class RestoreManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyInputsCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(
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
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Ids", Message = $"Không tìm thấy {missingIds.Count} phiếu nhập đã xóa: {string.Join(", ", missingIds)}" } ]
            };
        }

        updateRepository.Restore(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
