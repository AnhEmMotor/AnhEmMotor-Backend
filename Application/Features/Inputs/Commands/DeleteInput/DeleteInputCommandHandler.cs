using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed class DeleteInputCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInputCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteInputCommand request, CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(input is null)
        {
            return new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail { Field = "Id", Message = $"Không tìm thấy phiếu nhập có ID {request.Id}." } ]
            };
        }

        if(Domain.Constants.InputStatus.IsCannotDelete(input.StatusId))
        {
            return new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail
                    {
                        Field = "StatusId",
                        Message = $"Không thể xóa đơn hàng có trạng thái '{input.StatusId}'."
                    } ]
            };
        }

        deleteRepository.Delete(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
