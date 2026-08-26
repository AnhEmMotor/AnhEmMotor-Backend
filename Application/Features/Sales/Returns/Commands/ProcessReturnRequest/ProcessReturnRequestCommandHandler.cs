using Application.ApiContracts.Sales.Returns.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ReturnRequest;
using Application.Interfaces.Services.Shipping;
using Domain.Entities;
using Domain.Entities.Logistics;
using Domain.Enums;
using MediatR;

namespace Application.Features.Sales.Returns.Commands.ProcessReturnRequest;

public class ProcessReturnRequestCommandHandler : IRequestHandler<ProcessReturnRequestCommand, Result<ReturnRequestResponse>>
{
    private readonly IReturnRequestReadRepository _readRepository;
    private readonly IReturnRequestWriteRepository _writeRepository;
    private readonly IInventoryReceiptInsertRepository _inventoryReceiptInsertRepository;
    private readonly IShipmentReadRepository _shipmentReadRepository;
    private readonly IShipmentInsertRepository _shipmentInsertRepository;
    private readonly IShipmentUpdateRepository _shipmentUpdateRepository;
    private readonly IOutputReadRepository _outputReadRepository;
    private readonly IShippingService _shippingService;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessReturnRequestCommandHandler(
        IReturnRequestReadRepository readRepository,
        IReturnRequestWriteRepository writeRepository,
        IInventoryReceiptInsertRepository inventoryReceiptInsertRepository,
        IShipmentReadRepository shipmentReadRepository,
        IShipmentInsertRepository shipmentInsertRepository,
        IShipmentUpdateRepository shipmentUpdateRepository,
        IOutputReadRepository outputReadRepository,
        IShippingService shippingService,
        IUnitOfWork unitOfWork)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _inventoryReceiptInsertRepository = inventoryReceiptInsertRepository;
        _shipmentReadRepository = shipmentReadRepository;
        _shipmentInsertRepository = shipmentInsertRepository;
        _shipmentUpdateRepository = shipmentUpdateRepository;
        _outputReadRepository = outputReadRepository;
        _shippingService = shippingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReturnRequestResponse>> Handle(ProcessReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var returnRequest = await _readRepository.GetByIdAsync(request.ReturnRequestId, cancellationToken);
        if (returnRequest == null)
        {
            return Result<ReturnRequestResponse>.Failure("Return request not found");
        }

        bool deferredRestock = false;
        if (request.Status == "completed")
        {
            var output = await _outputReadRepository.GetByIdAsync(returnRequest.OrderId, cancellationToken, Domain.Constants.DataFetchMode.All);
            if (output != null)
            {
                // Gọi GHN tạo đơn thu hồi (từ địa chỉ khách/đơn hàng về Kho / Showroom)
                var carrierResult = await _shippingService.CreateReturnPickupOrderAsync(output, returnRequest, cancellationToken);
                if (carrierResult.IsSuccess && !string.IsNullOrWhiteSpace(carrierResult.Value))
                {
                    var returnShipment = new Shipment
                    {
                        TrackingNumber = carrierResult.Value,
                        Carrier = "Giao Hàng Nhanh",
                        CustomerName = returnRequest.CustomerName,
                        CustomerPhone = returnRequest.CustomerPhone,
                        CodAmount = 0,
                        ShippingCost = 0,
                        OriginAddress = output.CustomerAddress ?? "Địa chỉ khách hàng",
                        DestinationAddress = "Kho Anh Em Motor, Biên Hoà, Đồng Nai",
                        OriginLatitude = 10.7626,
                        OriginLongitude = 106.6602,
                        DestinationLatitude = Domain.Constants.Logistics.LogisticsConstants.DefaultShowroomLatitude,
                        DestinationLongitude = Domain.Constants.Logistics.LogisticsConstants.DefaultShowroomLongitude,
                        Type = Domain.Constants.Logistics.ShipmentType.ReturnDelivery,
                        OutputId = output.Id,
                        Status = ParcelDeliveryStatus.Shipping,
                        Items = returnRequest.Items.Select(i => new ShipmentItem
                        {
                            Quantity = i.Quantity,
                            ProductVariantId = null,
                        }).ToList()
                    };
                    await _shipmentInsertRepository.AddAsync(returnShipment, cancellationToken);
                    returnRequest.OriginalTrackingNumber = carrierResult.Value;
                    deferredRestock = true; // Hàng sẽ được nhập kho khi Webhook GHN báo hàng hoàn về kho
                }
            }
        }

        returnRequest.Status = request.Status;
        returnRequest.ReturnAction = request.ReturnAction;
        returnRequest.RejectionReason = request.RejectionReason;
        returnRequest.Note = request.Note;

        if (request.Status == "completed" && request.ReturnAction == "restock" && !deferredRestock)
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
