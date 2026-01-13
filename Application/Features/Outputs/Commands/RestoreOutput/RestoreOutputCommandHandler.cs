using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public sealed class RestoreOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreOutputCommand, Result<OutputResponse?>>
{
    public async Task<Result<OutputResponse?>> Handle(RestoreOutputCommand request, CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng đã xóa có ID {request.Id}.", "Id");
        }

        updateRepository.Restore(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return output.Adapt<OutputResponse>();
    }
}
