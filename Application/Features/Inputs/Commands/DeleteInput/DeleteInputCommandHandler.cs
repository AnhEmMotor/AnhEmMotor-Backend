using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed class DeleteInputCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInputCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(input is null)
        {
            return new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Không tìm thấy phiếu nhập có ID {request.Id}."
                    } ]
            };
        }

        if(Domain.Constants.InputStatus.IsCannotDelete(input.StatusId))
        {
            return new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
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
