using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed class DeleteManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyInputsCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var inputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if(inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Ids",
                        Message = $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}"
                    } ]
            };
        }

        var errors = new List<Common.Models.ErrorDetail>();

        foreach(var output in inputsList)
        {
            if(Domain.Constants.InputStatus.IsCannotDelete(output.StatusId))
            {
                errors.Add(
                    new Common.Models.ErrorDetail
                    {
                        Field = "Ids",
                        Message = $"Phiếu nhập với Id {output.Id} đã bị xóa trước đó"
                    });
            }
        }

        if(errors.Count > 0)
        {
            return new Common.Models.ErrorResponse { Errors = errors };
        }

        deleteRepository.Delete(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
