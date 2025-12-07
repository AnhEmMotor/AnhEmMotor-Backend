using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateManyInputStatus;

public sealed class UpdateManyInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyInputStatusCommand, (List<InputResponse>? data, Common.Models.ErrorResponse? error)>
{
    public async Task<(List<InputResponse>? data, Common.Models.ErrorResponse? error)> Handle(
        UpdateManyInputStatusCommand request,
        CancellationToken cancellationToken)
    {
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

        var inputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if(inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Ids",
                        Message = $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}"
                    } ]
            });
        }

        var errors = new List<Common.Models.ErrorDetail>();

        foreach(var input in inputsList)
        {
            if(InputStatus.IsCannotEdit(input.StatusId))
            {
                errors.Add(
                    new Common.Models.ErrorDetail
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
            return (null, new Common.Models.ErrorResponse { Errors = errors });
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
