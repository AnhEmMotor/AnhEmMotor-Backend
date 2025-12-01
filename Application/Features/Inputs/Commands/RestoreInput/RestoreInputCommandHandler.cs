using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Enums;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreInput;

public sealed class RestoreInputCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreInputCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(
        RestoreInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if (input is null)
        {
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Id", Message = $"Không tìm thấy phiếu nhập đã xóa có ID {request.Id}." } ]
            };
        }

        updateRepository.Restore(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
