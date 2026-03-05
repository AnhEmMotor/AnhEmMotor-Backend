using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInputNotes;

public sealed class UpdateInputNotesCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputNotesCommand, Result<InputDetailResponse>>
{
    public async Task<Result<InputDetailResponse>> Handle(
        UpdateInputNotesCommand request,
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

        input.Notes = request.Notes;

        updateRepository.Update(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(input.Id, cancellationToken).ConfigureAwait(false);

        return updated!.Adapt<InputDetailResponse>();
    }
}
