using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Invoice;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Client.Vehicles.Queries.GetCustomerVehicleHistory;

public class GetCustomerVehicleHistoryQueryHandler(
    IVehicleReadRepository vehicleRepository,
    IMaintenanceHistoryReadRepository maintenanceRepository,
    IInvoiceReadRepository invoiceRepository) : IRequestHandler<GetCustomerVehicleHistoryQuery, Result<CustomerVehicleHistoryResponse>>
{
    public async Task<Result<CustomerVehicleHistoryResponse>> Handle(
        GetCustomerVehicleHistoryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
            if (vehicle == null || vehicle.UserId != request.UserId)
            {
                return Result<CustomerVehicleHistoryResponse>.Failure(
                    Error.NotFound("Không tìm thấy thông tin xe.", "VehicleId"));
            }
            var response = new CustomerVehicleHistoryResponse();
            var allInvoices = await invoiceRepository.GetByUserIdAsync(vehicle.UserId!.Value.ToString(), cancellationToken);
            var invoice = allInvoices.FirstOrDefault(
                i => i.ChassisNo == vehicle.VinNumber || i.ChassisNo == vehicle.EngineNumber);
            if (invoice != null)
            {
                response.PurchaseHistory
                    .Add(
                        new PurchaseHistoryDto
                        {
                            Id = invoice.Id,
                            PurchaseDate = invoice.IssueDate,
                            InvoiceNumber = invoice.InvoiceNumber,
                            SellerName = invoice.SalesPerson ?? "AnhEmMotor Showroom",
                            Amount = invoice.TotalAmount,
                            Notes = invoice.PaymentMethod
                        });
            } else
            {
                response.PurchaseHistory
                    .Add(
                        new PurchaseHistoryDto
                        {
                            Id = 1,
                            PurchaseDate = vehicle.PurchaseDate.DateTime,
                            InvoiceNumber = "HD-" + vehicle.VinNumber,
                            SellerName = "AnhEmMotor Showroom",
                            Amount = vehicle.ImportPrice > 0 ? vehicle.ImportPrice : 89500000m,
                            Notes = "Hóa đơn mua xe"
                        });
            }
            var histories = await maintenanceRepository.GetByVehicleIdAsync(vehicle.Id, cancellationToken);
            foreach (var h in histories.OrderBy(x => x.MaintenanceDate))
            {
                response.WarrantyHistory
                    .Add(
                        new WarrantyHistoryDto
                        {
                            Id = h.Id,
                            StartDate = h.MaintenanceDate.DateTime,
                            ProviderName = h.ServiceType ?? "Bảo dưỡng định kỳ",
                            PolicyNumber = h.MaintenanceNumber,
                            Description = h.Description,
                            CoverageAmount = h.TotalCost,
                            Status = "completed"
                        });
            }
            return Result<CustomerVehicleHistoryResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<CustomerVehicleHistoryResponse>.Failure(
                Error.Failure("Crash", ex.ToString()));
        }
    }
}
