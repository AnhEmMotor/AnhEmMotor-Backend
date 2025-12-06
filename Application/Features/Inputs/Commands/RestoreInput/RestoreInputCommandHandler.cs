using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreInput;

public sealed class RestoreInputCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreInputCommand, (InputResponse? Data, ErrorResponse? Error)>
{
    public async Task<(InputResponse? Data, ErrorResponse? Error)> Handle(
        RestoreInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(input is null)
        {
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Không tìm thấy phiếu nhập đã xóa có ID {request.Id}."
                    } ]
            });
        }

        updateRepository.Restore(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (input!.Adapt<InputResponse>(), null);
    }
}
