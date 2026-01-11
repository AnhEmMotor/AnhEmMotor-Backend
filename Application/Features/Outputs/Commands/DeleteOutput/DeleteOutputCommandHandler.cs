using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;

using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteOutput;

public sealed class DeleteOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteOutputCommand, Result>
{
    public async Task<Result> Handle(
        DeleteOutputCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(output is null)
        {
            return Result.Failure(Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id"));
        }

        if(OrderStatus.IsCannotDelete(output.StatusId))
        {
            return Result.Failure(Error.BadRequest($"Không thể xóa đơn hàng có trạng thái '{output.StatusId}'.", "StatusId"));
        }

        deleteRepository.Delete(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
