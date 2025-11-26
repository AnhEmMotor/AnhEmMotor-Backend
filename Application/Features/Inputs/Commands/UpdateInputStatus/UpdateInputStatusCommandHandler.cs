using Application.ApiContracts.Input;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Enums;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInputStatus;

public sealed class UpdateInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputStatusCommand, InputResponse>
{
    public async Task<InputResponse> Handle(
        UpdateInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if (input is null)
        {
            throw new InvalidOperationException($"Không tìm thấy phiếu nhập có ID {request.Id}.");
        }

        if (!InputStatus.IsValid(request.StatusId))
        {
            throw new InvalidOperationException($"Trạng thái '{request.StatusId}' không hợp lệ.");
        }

        input.StatusId = request.StatusId;
        updateRepository.Update(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(
            input.Id,
            cancellationToken)
            .ConfigureAwait(false);

        return updated!.Adapt<InputResponse>();
    }
}
