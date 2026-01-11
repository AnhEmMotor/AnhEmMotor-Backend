using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInputStatus;

public sealed class UpdateInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputStatusCommand, Result<InputResponse>>
{
    public async Task<Result<InputResponse>> Handle(
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
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id");
        }

        if(InputStatus.IsCannotEdit(input.StatusId))
        {
            return Error.BadRequest("Không thể sửa trạng thái phiếu nhập đã hoàn thành hoặc đã hủy.", "StatusId");
        }

        if(!InputStatus.IsValid(request.StatusId))
        {
            return Error.BadRequest($"Trạng thái '{request.StatusId}' không hợp lệ.", "StatusId");
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

        return updated.Adapt<InputResponse>();
    }
}
