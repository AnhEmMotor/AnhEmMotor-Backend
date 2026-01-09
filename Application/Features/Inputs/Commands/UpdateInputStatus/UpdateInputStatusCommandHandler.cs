using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Common.Models;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInputStatus;

public sealed class UpdateInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputStatusCommand, (InputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(InputResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(input is null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Không tìm thấy phiếu nhập có ID {request.Id}."
                    } ]
            });
        }

        if(InputStatus.IsCannotEdit(input.StatusId))
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "StatusId",
                        Message = "Không thể sửa trạng thái phiếu nhập đã hoàn thành hoặc đã hủy."
                    } ]
            });
        }

        if(!InputStatus.IsValid(request.StatusId))
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "StatusId",
                        Message = $"Trạng thái '{request.StatusId}' không hợp lệ."
                    } ]
            });
        }

        input.StatusId = request.StatusId;

        if(string.Equals(request.StatusId, InputStatus.Finish, StringComparison.OrdinalIgnoreCase))
        {
            input.InputDate = DateTimeOffset.UtcNow;
            input.CreatedBy = request.CurrentUserId;
        }

        updateRepository.Update(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(input.Id, cancellationToken).ConfigureAwait(false);

        return (updated!.Adapt<InputResponse>(), null);
    }
}
