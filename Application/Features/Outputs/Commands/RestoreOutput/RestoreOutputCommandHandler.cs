using Application.ApiContracts.Output;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public sealed class RestoreOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreOutputCommand, (OutputResponse? Data, ErrorResponse? Error)>
{
    public async Task<(OutputResponse? Data, ErrorResponse? Error)> Handle(
        RestoreOutputCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(output is null)
        {
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail { Field = "Id", Message = $"Không tìm thấy đơn hàng đã xóa có ID {request.Id}." } ]
            });
        }

        updateRepository.Restore(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (output.Adapt<OutputResponse>(), null);
    }
}
