using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;

using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed class DeleteManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyInputsCommand, Result>
{
    public async Task<Result> Handle(
        DeleteManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var inputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if(inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return Result.Failure(Error.NotFound($"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}", "Ids"));
        }

        var errors = new List<Error>();

        foreach(var output in inputsList)
        {
            if(Domain.Constants.InputStatus.IsCannotDelete(output.StatusId))
            {
                errors.Add(Error.BadRequest($"Phiếu nhập với Id {output.Id} đã bị xóa trước đó", "Ids"));
            }
        }

        if(errors.Count > 0)
        {
            return Result.Failure(errors);
        }

        deleteRepository.Delete(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
