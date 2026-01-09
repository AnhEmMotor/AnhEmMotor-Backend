using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Common.Models;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreInput;

public sealed class RestoreInputCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreInputCommand, (InputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(InputResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        RestoreInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(input is null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
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
