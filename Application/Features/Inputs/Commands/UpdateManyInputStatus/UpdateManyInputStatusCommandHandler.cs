using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateManyInputStatus;

public sealed class UpdateManyInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyInputStatusCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateManyInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!InputStatus.IsValid(request.StatusId))
        {
            throw new InvalidOperationException($"Trạng thái '{request.StatusId}' không hợp lệ.");
        }

        var inputs = await readRepository.GetByIdAsync(
            request.Ids,
            cancellationToken)
            .ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if (inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            throw new InvalidOperationException(
                $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}");
        }

        foreach (var input in inputsList)
        {
            input.StatusId = request.StatusId;
            updateRepository.Update(input);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
