using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed class RestoreManyOutputsCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyOutputsCommand, Result<List<OutputResponse>?>>
{
    public async Task<Result<List<OutputResponse>?>> Handle(
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
            return Error.NotFound(
                $"Không tìm thấy {missingIds.Count} đơn hàng đã xóa: {string.Join(", ", missingIds)}",
                "Ids");
        }

        updateRepository.Restore(outputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return updateRepository.Adapt<List<OutputResponse>>();
    }
}
