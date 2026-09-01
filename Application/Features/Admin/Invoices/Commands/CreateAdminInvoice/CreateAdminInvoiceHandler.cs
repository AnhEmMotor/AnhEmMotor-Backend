using Application.ApiContracts.Admin.Invoices;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Invoice;
using Application.Interfaces.Repositories.Voucher;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Admin.Invoices.Commands.CreateAdminInvoice;

public record CreateAdminInvoiceCommand(CreateAdminInvoiceRequest Request) : IRequest<Result<AdminInvoiceDetailResponse>>;

public class CreateAdminInvoiceHandler(
    IInvoiceWriteRepository writeRepo,
    IInvoiceReadRepository readRepo,
    IVoucherReadRepository voucherReadRepo,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateAdminInvoiceCommand, Result<AdminInvoiceDetailResponse>>
{
    public async Task<Result<AdminInvoiceDetailResponse>> Handle(
        CreateAdminInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var req = request.Request;
        var invoiceNumber = GenerateInvoiceNumber();
        var userId = currentUserContext.GetUserId();

        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(req.VoucherCode))
        {
            var voucher = await voucherReadRepo.GetByCodeAsync(req.VoucherCode, cancellationToken);
            if (voucher != null)
            {
                if (voucher.DiscountType == DiscountType.Percent)
                {
                    discount = (req.VehiclePrice * voucher.DiscountValue) / 100m;
                    if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
                    {
                        discount = voucher.MaxDiscountAmount.Value;
                    }
                }
                else
                {
                    discount = voucher.DiscountValue;
                }
            }
        }

        var totalAmount = Math.Max(0, req.VehiclePrice - discount) + req.RegistrationFee + req.InsuranceFee;

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            IssueDate = DateTime.Now,
            UserId = userId,
            CustomerName = req.CustomerName,
            CustomerPhone = req.CustomerPhone,
            CustomerIdCard = req.CustomerIdCard,
            CustomerAddress = req.CustomerAddress,
            VehicleModel = req.VehicleModel,
            VehicleVersion = req.VehicleVersion,
            VehicleColor = req.VehicleColor,
            VehicleType = req.VehicleType,
            VehicleImage = req.VehicleImage,
            ChassisNo = req.ChassisNo,
            EngineNo = req.EngineNo,
            VehiclePrice = req.VehiclePrice,
            RegistrationFee = req.RegistrationFee,
            InsuranceFee = req.InsuranceFee,
            VoucherCode = req.VoucherCode,
            DepositPercentage = req.DepositPercentage ?? 100,
            TotalAmount = totalAmount,
            PaymentMethod = req.PaymentMethod,
            BankName = req.BankName ?? string.Empty,
            Status = "pending",
            SalesPerson = req.SalesPerson ?? string.Empty,
            DeliveryDate = req.DeliveryDate,
            CreatedAt = DateTimeOffset.Now
        };
        writeRepo.Add(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var created = await readRepo.GetByIdAsync(invoice.Id, cancellationToken);
        if (created == null)
            return Result<AdminInvoiceDetailResponse>.Failure(Error.BadRequest("Không thể tạo hóa đơn", "Create"));
        var response = new AdminInvoiceDetailResponse(
            created.Id,
            created.InvoiceNumber,
            created.IssueDate,
            created.CustomerName,
            created.CustomerPhone,
            created.CustomerIdCard,
            created.CustomerAddress,
            created.VehicleModel,
            created.VehicleVersion,
            created.VehicleColor,
            created.VehicleType,
            created.VehicleImage,
            created.ChassisNo,
            created.EngineNo,
            created.VehiclePrice,
            created.RegistrationFee,
            created.InsuranceFee,
            created.VoucherCode,
            created.DepositPercentage,
            created.TotalAmount,
            created.PaymentMethod,
            created.BankName,
            created.Status,
            created.SalesPerson,
            created.DeliveryDate,
            created.ProcessedBy,
            created.ProcessedAt,
            created.CreatedAt,
            new List<InvoicePaymentBreakdownItem>());
        return Result<AdminInvoiceDetailResponse>.Success(response);
    }

    private static string GenerateInvoiceNumber()
    {
        var now = DateTime.Now;
        var datePart = now.ToString("yyyyMMdd");
        var guidPart = Guid.NewGuid().ToString("N")[..8];
        return $"HD-{datePart}-{guidPart}";
    }
}
