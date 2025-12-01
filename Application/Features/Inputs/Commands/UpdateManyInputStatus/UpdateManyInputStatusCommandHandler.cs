using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateManyInputStatus;

public sealed class UpdateManyInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyInputStatusCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(
        UpdateManyInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!InputStatus.IsValid(request.StatusId))
        {
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "StatusId", Message = $"Trạng thái '{request.StatusId}' không hợp lệ." } ]
            };
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
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Ids", Message = $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}" } ]
            };
        }

        foreach (var input in inputsList)
        {
            input.StatusId = request.StatusId;
            updateRepository.Update(input);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
