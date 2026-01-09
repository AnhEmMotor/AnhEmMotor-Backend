using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed class RestoreManyOutputsCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyOutputsCommand, (List<OutputResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<OutputResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
        RestoreManyOutputsCommand request,
        CancellationToken cancellationToken)
    {
        var outputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var outputsList = outputs.ToList();

        if(outputsList.Count != request.Ids.Count)
        {
            var foundIds = outputsList.Select(o => o.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Ids",
                        Message = $"Không tìm thấy {missingIds.Count} đơn hàng đã xóa: {string.Join(", ", missingIds)}"
                    } ]
            });
        }

        updateRepository.Restore(outputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (updateRepository.Adapt<List<OutputResponse>>(), null);
    }
}
