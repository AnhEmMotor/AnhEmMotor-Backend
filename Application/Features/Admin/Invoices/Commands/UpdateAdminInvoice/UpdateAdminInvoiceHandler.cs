using Application.ApiContracts.Admin.Invoices;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Invoice;
using Domain.Entities;
using MediatR;

namespace Application.Features.Admin.Invoices.Commands.UpdateAdminInvoice;

public record UpdateAdminInvoiceCommand(int Id, UpdateAdminInvoiceRequest Request) : IRequest<Result<AdminInvoiceDetailResponse>>;

public class UpdateAdminInvoiceHandler(IInvoiceWriteRepository writeRepo, IInvoiceReadRepository readRepo, IUnitOfWork unitOfWork) : IRequestHandler<UpdateAdminInvoiceCommand, Result<AdminInvoiceDetailResponse>>
{
    public async Task<Result<AdminInvoiceDetailResponse>> Handle(UpdateAdminInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await readRepo.GetByIdAsync(request.Id, cancellationToken);
        if (invoice == null)
            return Result<AdminInvoiceDetailResponse>.Failure(Error.NotFound("Không tìm thấy hóa đơn", "Id"));

        var req = request.Request;

        invoice.CustomerName = req.CustomerName;
        invoice.CustomerPhone = req.CustomerPhone;
        invoice.CustomerIdCard = req.CustomerIdCard;
        invoice.CustomerAddress = req.CustomerAddress;
        invoice.VehicleModel = req.VehicleModel;
        invoice.VehicleColor = req.VehicleColor;
        invoice.ChassisNo = req.ChassisNo;
        invoice.EngineNo = req.EngineNo;
        invoice.VehiclePrice = req.VehiclePrice;
        invoice.RegistrationFee = req.RegistrationFee;
        invoice.InsuranceFee = req.InsuranceFee;
        invoice.TotalAmount = req.VehiclePrice + req.RegistrationFee + req.InsuranceFee;
        invoice.PaymentMethod = req.PaymentMethod;
        invoice.BankName = req.BankName;
        invoice.Status = req.Status;
        invoice.SalesPerson = req.SalesPerson;
        invoice.DeliveryDate = req.DeliveryDate;
        invoice.UpdatedAt = DateTimeOffset.Now;

        writeRepo.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await readRepo.GetByIdAsync(request.Id, cancellationToken);
        if (updated == null)
            return Result<AdminInvoiceDetailResponse>.Failure(Error.NotFound("Không tìm thấy hóa đơn sau cập nhật", "Id"));

        var response = new AdminInvoiceDetailResponse(
            updated.Id,
            updated.InvoiceNumber,
            updated.IssueDate,
            updated.CustomerName,
            updated.CustomerPhone,
            updated.CustomerIdCard,
            updated.CustomerAddress,
            updated.VehicleModel,
            updated.VehicleColor,
            updated.ChassisNo,
            updated.EngineNo,
            updated.VehiclePrice,
            updated.RegistrationFee,
            updated.InsuranceFee,
            updated.TotalAmount,
            updated.PaymentMethod,
            updated.BankName,
            updated.Status,
            updated.SalesPerson,
            updated.DeliveryDate,
            updated.ProcessedBy,
            updated.ProcessedAt,
            updated.CreatedAt,
            new List<InvoicePaymentBreakdownItem>()
        );

        return Result<AdminInvoiceDetailResponse>.Success(response);
    }
}
