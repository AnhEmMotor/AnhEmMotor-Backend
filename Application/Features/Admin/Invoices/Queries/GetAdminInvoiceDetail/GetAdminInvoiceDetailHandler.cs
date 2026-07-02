using Application.ApiContracts.Admin.Invoices;
using Application.Common.Models;
using Application.Interfaces.Repositories.Invoice;
using Domain.Primitives;
using InvoiceEntity = Domain.Entities.Invoice;
using MediatR;

namespace Application.Features.Admin.Invoices.Queries.GetAdminInvoiceDetail;

public record GetAdminInvoiceDetailQuery(int Id) : IRequest<Result<AdminInvoiceDetailResponse>>;

public class GetAdminInvoiceDetailHandler(IInvoiceReadRepository repository) : IRequestHandler<GetAdminInvoiceDetailQuery, Result<AdminInvoiceDetailResponse>>
{
    public async Task<Result<AdminInvoiceDetailResponse>> Handle(GetAdminInvoiceDetailQuery request, CancellationToken cancellationToken)
    {
        var invoice = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (invoice == null)
            return Result<AdminInvoiceDetailResponse>.Failure(Error.NotFound("Không tìm thấy hóa đơn", "Id"));

        var response = new AdminInvoiceDetailResponse(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.IssueDate,
            invoice.CustomerName,
            invoice.CustomerPhone,
            invoice.CustomerIdCard,
            invoice.CustomerAddress,
            invoice.VehicleModel,
            invoice.VehicleColor,
            invoice.ChassisNo,
            invoice.EngineNo,
            invoice.VehiclePrice,
            invoice.RegistrationFee,
            invoice.InsuranceFee,
            invoice.TotalAmount,
            invoice.PaymentMethod,
            invoice.BankName,
            invoice.Status,
            invoice.SalesPerson,
            invoice.DeliveryDate,
            invoice.ProcessedBy,
            invoice.ProcessedAt,
            invoice.CreatedAt,
            new List<InvoicePaymentBreakdownItem>()
        );

        return Result<AdminInvoiceDetailResponse>.Success(response);
    }
}
