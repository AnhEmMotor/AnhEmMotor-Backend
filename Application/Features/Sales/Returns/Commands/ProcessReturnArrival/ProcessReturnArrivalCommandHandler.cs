using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Entities;
using MediatR;

namespace Application.Features.Sales.Returns.Commands.ProcessReturnArrival;

public class ProcessReturnArrivalCommandHandler(
    IReturnRequestReadRepository readRepository,
    IReturnRequestWriteRepository writeRepository,
    IInventoryReceiptInsertRepository receiptInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ProcessReturnArrivalCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ProcessReturnArrivalCommand request, CancellationToken cancellationToken)
    {
        var returnRequests = await readRepository
            .GetByOrderIdAsync(request.OutputId, cancellationToken)
            .ConfigureAwait(false);

        var activeOrCompletedReturns = returnRequests
            .Where(x => x.Status != "rejected")
            .ToList();

        if (activeOrCompletedReturns.Count == 0)
        {
            return Result<int>.Success(0);
        }

        int processedCount = 0;
        foreach (var returnRequest in activeOrCompletedReturns)
        {
            // Tự động chuyển trạng thái đơn hoàn sang completed nếu chưa completed
            if (returnRequest.Status != "completed")
            {
                returnRequest.Status = "completed";
                returnRequest.ReturnAction ??= "restock";
                await writeRepository.UpdateAsync(returnRequest, cancellationToken).ConfigureAwait(false);
            }

            if (returnRequest.ReturnAction == "restock")
            {
                var receipt = new InventoryReceipt
                {
                    InventoryReceiptDate = DateTimeOffset.UtcNow,
                    Notes = $"Restock from Return Request #{returnRequest.Id}",
                    StatusId = Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve,
                    SourceOrderId = returnRequest.OrderId,
                    InventoryReceiptInfos = returnRequest.Items.Select(i => new InventoryReceiptInfo
                    {
                        Count = i.Quantity,
                        RemainingCount = i.Quantity
                    }).ToList()
                };
                receiptInsertRepository.Add(receipt);
                processedCount++;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(processedCount);
    }
}
