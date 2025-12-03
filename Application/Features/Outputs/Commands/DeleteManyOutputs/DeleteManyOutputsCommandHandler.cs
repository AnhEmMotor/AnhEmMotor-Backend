using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteManyOutputs;

public sealed class DeleteManyOutputsCommandHandler(
    IOutputReadRepository readRepository,
    IOutputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyOutputsCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteManyOutputsCommand request, CancellationToken cancellationToken)
    {
        var outputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        var outputsList = outputs.ToList();

        if(outputsList.Count != request.Ids.Count)
        {
            var foundIds = outputsList.Select(o => o.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail
                    {
                        Field = "Ids",
                        Message = $"Không tìm thấy {missingIds.Count} đơn hàng: {string.Join(", ", missingIds)}"
                    } ]
            };
        }

        var errors = new List<ErrorDetail>();

        foreach(var output in outputsList)
        {
            if(Domain.Constants.OrderStatus.IsCannotDelete(output.StatusId))
            {
                errors.Add(
                    new ErrorDetail { Field = "Ids", Message = $"Đơn hàng với Id {output.Id} đã bị xóa trước đó" });
            }
        }

        if(errors.Count > 0)
        {
            return new ErrorResponse { Errors = errors };
        }

        deleteRepository.Delete(outputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
