using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteOutput;

public sealed class DeleteOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteOutputCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteOutputCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(output is null)
        {
            return new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Không tìm thấy đơn hàng có ID {request.Id}."
                    } ]
            };
        }

        if(Domain.Constants.OrderStatus.IsCannotDelete(output.StatusId))
        {
            return new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "StatusId",
                        Message = $"Không thể xóa đơn hàng có trạng thái '{output.StatusId}'."
                    } ]
            };
        }

        deleteRepository.Delete(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
