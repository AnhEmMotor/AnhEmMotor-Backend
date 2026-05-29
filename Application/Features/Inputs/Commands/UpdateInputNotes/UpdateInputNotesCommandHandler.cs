using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.UpdateInputNotes;

public sealed class UpdateInputNotesCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputNotesCommand, Result<InputDetailResponse>>
{
    public async Task<Result<InputDetailResponse>> Handle(
        UpdateInputNotesCommand request,
        CancellationToken cancellationToken)
    {
        var InventoryReceipt = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Error.NotFound($"Kh�ng t�m th?y phi?u nh?p c� ID {request.Id}.", "Id");
        }
        InventoryReceipt.Notes = request.Notes;
        updateRepository.Update(InventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var updated = await readRepository.GetByIdWithDetailsAsync(InventoryReceipt.Id, cancellationToken).ConfigureAwait(false);
        return updated!.Adapt<InputDetailResponse>();
    }
}
