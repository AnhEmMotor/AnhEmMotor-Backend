using Application.Common.Models;
using Application.Features.InventoryOnHand.Notifications;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteOutput;

public class DeleteOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork,
    IPublisher? publisher = null) : IRequestHandler<DeleteOutputCommand, Result>
{
    public async Task<Result> Handle(DeleteOutputCommand request, CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (output is null)
        {
            return Result.Failure(Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id"));
        }
        if (OrderStatus.IsCannotDelete(output.StatusId))
        {
            return Result.Failure(
                Error.BadRequest($"Không thể xóa đơn hàng có trạng thái '{output.StatusId}'.", "StatusId"));
        }
        output.DeletedAt = DateTimeOffset.UtcNow;
        deleteRepository.Delete(output);
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
        return Result.Success();
    }
}
