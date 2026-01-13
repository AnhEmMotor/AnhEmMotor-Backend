using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;

using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed class DeleteInputCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInputCommand, Result>
{
    public async Task<Result> Handle(DeleteInputCommand request, CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(request.Id!.Value, cancellationToken).ConfigureAwait(false);

        if(input is null)
        {
            return Result.Failure(Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id"));
        }

        if(Domain.Constants.Input.InputStatus.IsCannotDelete(input.StatusId))
        {
            return Result.Failure(
                Error.BadRequest($"Không thể xóa đơn hàng có trạng thái '{input.StatusId}'.", "StatusId"));
        }

        deleteRepository.Delete(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

