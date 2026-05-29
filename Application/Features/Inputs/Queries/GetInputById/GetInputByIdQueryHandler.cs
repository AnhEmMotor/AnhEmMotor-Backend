using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;

using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInputById;

public sealed class GetInputByIdQueryHandler(IInputReadRepository repository) : IRequestHandler<GetInputByIdQuery, Result<InputDetailResponse?>>
{
    public async Task<Result<InputDetailResponse?>> Handle(
        GetInputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var InventoryReceipt = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.");
        }
        return InventoryReceipt.Adapt<InputDetailResponse>();
    }
}
