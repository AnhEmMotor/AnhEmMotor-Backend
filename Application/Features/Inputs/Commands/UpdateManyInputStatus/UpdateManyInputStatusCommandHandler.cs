using Application.ApiContracts.Input;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateManyInputStatus;

public sealed class UpdateManyInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyInputStatusCommand, (List<InputResponse>? data, ErrorResponse? error)>
{
    public async Task<(List<InputResponse>? data, ErrorResponse? error)> Handle(
        UpdateManyInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        if(!InputStatus.IsValid(request.StatusId))
        {
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail { Field = "StatusId", Message = $"Trạng thái '{request.StatusId}' không hợp lệ." } ]
            });
        }

        var inputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if(inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail
                    {
                        Field = "Ids",
                        Message = $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}"
                    } ]
            });
        }

        var errors = new List<ErrorDetail>();

        foreach(var input in inputsList)
        {
            if(InputStatus.IsCannotEdit(input.StatusId))
            {
                errors.Add(
                    new ErrorDetail
                    {
                        Field = "Ids",
                        Message =
                            $"Phiếu nhập {input.Id} đang ở trạng thái '{input.StatusId}' nên không thể chuyển sang '{request.StatusId}'."
                    });
                continue;
            }
        }

        if(errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }
        foreach(var input in inputsList)
        {
            input.StatusId = request.StatusId;
            updateRepository.Update(input);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (inputs.Adapt<List<InputResponse>>(), null);
    }
}
