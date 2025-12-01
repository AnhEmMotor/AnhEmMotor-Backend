using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Enums;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public sealed class RestoreOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreOutputCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(
        RestoreOutputCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if (output is null)
        {
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Id", Message = $"Không tìm thấy đơn hàng đã xóa có ID {request.Id}." } ]
            };
        }

        updateRepository.Restore(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
