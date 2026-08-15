using Application.ApiContracts.Sales.Returns.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Entities;
using MediatR;

namespace Application.Features.Sales.Returns.Commands.ProcessReturnRequest;

public class ProcessReturnRequestCommandHandler : IRequestHandler<ProcessReturnRequestCommand, Result<ReturnRequestResponse>>
{
    private readonly IReturnRequestReadRepository _readRepository;
    private readonly IReturnRequestWriteRepository _writeRepository;
    private readonly IInventoryReceiptInsertRepository _inventoryReceiptInsertRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessReturnRequestCommandHandler(
        IReturnRequestReadRepository readRepository,
        IReturnRequestWriteRepository writeRepository,
        IInventoryReceiptInsertRepository inventoryReceiptInsertRepository,
        IUnitOfWork unitOfWork)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _inventoryReceiptInsertRepository = inventoryReceiptInsertRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReturnRequestResponse>> Handle(ProcessReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var returnRequest = await _readRepository.GetByIdAsync(request.ReturnRequestId, cancellationToken);
        if (returnRequest == null)
        {
            return Result<ReturnRequestResponse>.Failure("Return request not found");
        }

        returnRequest.Status = request.Status;
        returnRequest.ReturnAction = request.ReturnAction;
        returnRequest.RejectionReason = request.RejectionReason;
        returnRequest.Note = request.Note;

        if (request.Status == "completed" && request.ReturnAction == "restock")
        {
            var receipt = new InventoryReceipt
            {
                InventoryReceiptDate = DateTimeOffset.UtcNow,
                Notes = $"Restock from Return Request #{returnRequest.Id}",
                StatusId = "approved", 
                SourceOrderId = returnRequest.OrderId,
                InventoryReceiptInfos = returnRequest.Items.Select(i => new InventoryReceiptInfo
                {
                    Count = i.Quantity,
                    RemainingCount = i.Quantity
                }).ToList()
            };

            _inventoryReceiptInsertRepository.Add(receipt);
        }

        await _writeRepository.UpdateAsync(returnRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ReturnRequestResponse
        {
            Id = returnRequest.Id,
            OrderId = returnRequest.OrderId,
            OrderCode = returnRequest.OrderCode,
            CustomerName = returnRequest.CustomerName,
            CustomerPhone = returnRequest.CustomerPhone,
            Carrier = returnRequest.Carrier,
            OriginalTrackingNumber = returnRequest.OriginalTrackingNumber,
            Type = returnRequest.Type,
            Status = returnRequest.Status,
            Reason = returnRequest.Reason,
            Items = returnRequest.Items.Select(i => new ReturnRequestItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                ReturnQuantity = i.ReturnQuantity,
                UnitPrice = i.UnitPrice,
                Sku = i.Sku
            }).ToList()
        };

        return Result<ReturnRequestResponse>.Success(response);
    }
}
