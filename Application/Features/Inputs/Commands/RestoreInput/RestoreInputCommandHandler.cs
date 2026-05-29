using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.RestoreInput;

public sealed class RestoreInputCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreInputCommand, Result<InputDetailResponse>>
{
    public async Task<Result<InputDetailResponse>> Handle(
        RestoreInputCommand request,
        CancellationToken cancellationToken)
    {
        var InventoryReceipt = await readRepository.GetByIdAsync(request.Id!.Value, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Error.NotFound($"Kh�ng t�m th?y phi?u nh?p d� x�a c� ID {request.Id}.", "Id");
        }
        updateRepository.Restore(InventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return InventoryReceipt.Adapt<InputDetailResponse>();
    }
}

