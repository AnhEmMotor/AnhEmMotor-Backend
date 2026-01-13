using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;

using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteManyOutputs;

public sealed class DeleteManyOutputsCommandHandler(
    IOutputReadRepository readRepository,
    IOutputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyOutputsCommand, Result>
{
    public async Task<Result> Handle(DeleteManyOutputsCommand request, CancellationToken cancellationToken)
    {
        var outputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        var outputsList = outputs.ToList();

        if(outputsList.Count != request.Ids.Count)
        {
            var foundIds = outputsList.Select(o => o.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return Result.Failure(
                Error.NotFound($"Không tìm thấy {missingIds.Count} đơn hàng: {string.Join(", ", missingIds)}", "Ids"));
        }

        var errors = new List<Error>();

        foreach(var output in outputsList)
        {
            if(OrderStatus.IsCannotDelete(output.StatusId))
            {
                errors.Add(Error.BadRequest($"Đơn hàng với Id {output.Id} đã bị xóa trước đó", "Id"));
            }
        }

        if(errors.Count > 0)
        {
            return Result.Failure(errors);
        }

        deleteRepository.Delete(outputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
