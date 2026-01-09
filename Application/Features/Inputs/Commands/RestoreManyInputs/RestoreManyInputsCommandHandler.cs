using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Common.Models;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed class RestoreManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyInputsCommand, (List<InputResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<InputResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
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
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Ids",
                        Message =
                            $"Không tìm thấy {missingIds.Count} phiếu nhập đã xóa: {string.Join(", ", missingIds)}"
                    } ]
            });
        }

        updateRepository.Restore(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (inputs.Adapt<List<InputResponse>>(), null);
    }
}
