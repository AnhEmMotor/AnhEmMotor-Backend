using Application.ApiContracts.Admin.Invoices;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Invoice;
using Application.Interfaces.Repositories.Voucher;
using Domain.Enums;
using MediatR;

namespace Application.Features.Admin.Invoices.Commands.UpdateAdminInvoice;

public record UpdateAdminInvoiceCommand(int Id, UpdateAdminInvoiceRequest Request) : IRequest<Result<AdminInvoiceDetailResponse>>;

public class UpdateAdminInvoiceHandler(
    IInvoiceWriteRepository writeRepo,
    IInvoiceReadRepository readRepo,
    IVoucherReadRepository voucherReadRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateAdminInvoiceCommand, Result<AdminInvoiceDetailResponse>>
{
    public async Task<Result<AdminInvoiceDetailResponse>> Handle(
        UpdateAdminInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await readRepo.GetByIdAsync(request.Id, cancellationToken);
        if (invoice == null)
            return Result<AdminInvoiceDetailResponse>.Failure(Error.NotFound("Không tìm thấy hóa đơn", "Id"));
        var req = request.Request;

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

        invoice.CustomerName = req.CustomerName;
        invoice.CustomerPhone = req.CustomerPhone;
        invoice.CustomerIdCard = req.CustomerIdCard;
        invoice.CustomerAddress = req.CustomerAddress;
        invoice.VehicleModel = req.VehicleModel;
        invoice.VehicleVersion = req.VehicleVersion;
        invoice.VehicleColor = req.VehicleColor;
        invoice.VehicleType = req.VehicleType;
        invoice.VehicleImage = req.VehicleImage;
        invoice.ChassisNo = req.ChassisNo;
        invoice.EngineNo = req.EngineNo;
        invoice.VehiclePrice = req.VehiclePrice;
        invoice.RegistrationFee = req.RegistrationFee;
        invoice.InsuranceFee = req.InsuranceFee;
        invoice.VoucherCode = req.VoucherCode;
        invoice.DepositPercentage = req.DepositPercentage ?? 100;
        invoice.TotalAmount = totalAmount;
        invoice.PaymentMethod = req.PaymentMethod;
        invoice.BankName = req.BankName ?? string.Empty;
        invoice.Status = req.Status;
        invoice.SalesPerson = req.SalesPerson ?? string.Empty;
        invoice.DeliveryDate = req.DeliveryDate;
        invoice.UpdatedAt = DateTimeOffset.Now;
        writeRepo.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var updated = await readRepo.GetByIdAsync(request.Id, cancellationToken);
        if (updated == null)
            return Result<AdminInvoiceDetailResponse>.Failure(
                Error.NotFound("Không tìm thấy hóa đơn sau cập nhật", "Id"));
        var response = new AdminInvoiceDetailResponse(
            updated.Id,
            updated.InvoiceNumber,
            updated.IssueDate,
            updated.CustomerName,
            updated.CustomerPhone,
            updated.CustomerIdCard,
            updated.CustomerAddress,
            updated.VehicleModel,
            updated.VehicleVersion,
            updated.VehicleColor,
            updated.VehicleType,
            updated.VehicleImage,
            updated.ChassisNo,
            updated.EngineNo,
            updated.VehiclePrice,
            updated.RegistrationFee,
            updated.InsuranceFee,
            updated.VoucherCode,
            updated.DepositPercentage,
            updated.TotalAmount,
            updated.PaymentMethod,
            updated.BankName,
            updated.Status,
            updated.SalesPerson,
            updated.DeliveryDate,
            updated.ProcessedBy,
            updated.ProcessedAt,
            updated.CreatedAt,
            new List<InvoicePaymentBreakdownItem>());
        return Result<AdminInvoiceDetailResponse>.Success(response);
    }
}
