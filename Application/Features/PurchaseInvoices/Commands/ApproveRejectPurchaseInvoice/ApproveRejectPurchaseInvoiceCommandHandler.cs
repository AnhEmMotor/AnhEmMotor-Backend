using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseInvoices.Commands.ApproveRejectPurchaseInvoice
{
    public sealed class ApproveRejectPurchaseInvoiceCommandHandler(
        IPurchaseInvoiceReadRepository readRepository,
        IPurchaseInvoiceUpdateRepository updateRepository,
        ISupplierDebtRepository supplierDebtRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<ApproveRejectPurchaseInvoiceCommand, Result<PurchaseInvoiceDetailResponse?>>
    {
        public async Task<Result<PurchaseInvoiceDetailResponse?>> Handle(
            ApproveRejectPurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            var invoice = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (invoice is null)
            {
                return Error.NotFound($"Không tìm thấy hóa đơn mua hàng có ID {request.Id}.", "Id");
            }

            if (!string.Equals(invoice.Status, "draft", StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest("Chỉ có thể phê duyệt hoặc từ chối hóa đơn ở trạng thái nháp.", "Status");
            }

            var currentUserId = currentUserContext.GetUserId();

            if (request.IsApproved)
            {
                invoice.Status = "approved";
                invoice.ApprovedBy = currentUserId;

                // Tự động tạo công nợ nhà cung cấp (SupplierDebt)
                if (invoice.PurchaseOrder != null)
                {
                    var totalAmount = invoice.PurchaseInvoiceItems
                        .Sum(i => i.InvoicedQuantity * i.UnitPrice * (1 + i.TaxRate / 100));

                    var debt = new SupplierDebt
                    {
                        PurchaseInvoiceId = invoice.Id,
                        SupplierId = invoice.PurchaseOrder.SupplierId,
                        TotalAmount = totalAmount,
                        PaidAmount = 0
                    };

                    supplierDebtRepository.Add(debt);
                }
            }
            else
            {
                invoice.Status = "rejected";
                invoice.ApprovedBy = currentUserId;
                if (!string.IsNullOrEmpty(request.Note))
                {
                    invoice.Note = string.IsNullOrEmpty(invoice.Note) 
                        ? request.Note 
                        : $"{invoice.Note} | Lý do từ chối: {request.Note}";
                }
            }

            updateRepository.Update(invoice);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updated = await readRepository.GetByIdWithDetailsAsync(invoice.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<PurchaseInvoiceDetailResponse?>();
        }
    }
}
