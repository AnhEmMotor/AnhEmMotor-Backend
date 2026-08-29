using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Features.InventoryOnHand.Notifications;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public class RestoreOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork,
    IPublisher? publisher = null) : IRequestHandler<RestoreOutputCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        RestoreOutputCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);
        if (output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng đã xóa có ID {request.Id}.", "Id");
        }
        updateRepository.Restore(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var combos = new HashSet<(int VariantId, int? ColorId)>();
        foreach (var info in output.OutputInfos)
        {
            if (info.ProductVariantId.HasValue)
            {
                combos.Add((info.ProductVariantId.Value, info.ProductVariantColorId));
            }
        }
        if (publisher != null && combos.Count > 0)
        {
            await publisher.Publish(new InventoryChangedNotification(combos), cancellationToken)
                .ConfigureAwait(false);
        }
        return output.Adapt<OrderDetailResponse>();
    }
}
