using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateManyInputStatus;

public sealed class UpdateManyInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyInputStatusCommand, Result<List<InputResponse>?>>
{
    public async Task<Result<List<InputResponse>?>> Handle(
        UpdateManyInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        if(!Domain.Constants.Input.InputStatus.IsValid(request.StatusId))
        {
            return Error.BadRequest($"Trạng thái '{request.StatusId}' không hợp lệ.", "StatusId");
        }

        var inputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if(inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return Error.NotFound(
                $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}",
                "Ids");
        }

        var errors = new List<Error>();

        foreach(var input in inputsList)
        {
            if(Domain.Constants.Input.InputStatus.IsCannotEdit(input.StatusId))
            {
                errors.Add(
                    Error.BadRequest(
                        $"Phiếu nhập {input.Id} đang ở trạng thái '{input.StatusId}' nên không thể chuyển sang '{request.StatusId}'.",
                        "Ids"));
                continue;
            }
        }

        if(errors.Count > 0)
        {
            return errors;
        }

        foreach(var input in inputsList)
        {
            input.StatusId = request.StatusId;
            updateRepository.Update(input);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return inputs.Adapt<List<InputResponse>>();
    }
}
