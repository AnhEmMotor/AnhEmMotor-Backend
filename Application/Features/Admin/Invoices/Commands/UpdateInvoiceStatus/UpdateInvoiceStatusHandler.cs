using Application.ApiContracts.Admin.Invoices;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Invoice;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Admin.Invoices.Commands.UpdateInvoiceStatus;

public record UpdateInvoiceStatusCommand(int Id, UpdateInvoiceStatusRequest Request) : IRequest<Result<AdminInvoiceDetailResponse>>;

public class UpdateInvoiceStatusHandler(IInvoiceWriteRepository writeRepo, IInvoiceReadRepository readRepo, ICurrentUserContext currentUserContext, IUnitOfWork unitOfWork) : IRequestHandler<UpdateInvoiceStatusCommand, Result<AdminInvoiceDetailResponse>>
{
    public async Task<Result<AdminInvoiceDetailResponse>> Handle(UpdateInvoiceStatusCommand request, CancellationToken cancellationToken)
    {
        var invoice = await readRepo.GetByIdAsync(request.Id, cancellationToken);
        if (invoice == null)
            return Result<AdminInvoiceDetailResponse>.Failure(Error.NotFound("Không tìm thấy hóa đơn", "Id"));

        var newStatus = request.Request.Status;
        if (newStatus == "completed" && invoice.Status != "completed")
        {
            invoice.ProcessedBy = request.Request.ProcessedBy ?? currentUserContext.GetUserId().ToString();
            invoice.ProcessedAt = DateTime.UtcNow;
        }

        invoice.Status = newStatus;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

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
