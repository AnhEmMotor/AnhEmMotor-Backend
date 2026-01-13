using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed class RestoreManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyInputsCommand, Result<List<InputResponse>>>
{
    public async Task<Result<List<InputResponse>>> Handle(
        RestoreManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var inputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if(inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return Error.NotFound(
                $"Không tìm thấy {missingIds.Count} phiếu nhập đã xóa: {string.Join(", ", missingIds)}",
                "Ids");
        }

        updateRepository.Restore(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return inputs.Adapt<List<InputResponse>>();
    }
}
