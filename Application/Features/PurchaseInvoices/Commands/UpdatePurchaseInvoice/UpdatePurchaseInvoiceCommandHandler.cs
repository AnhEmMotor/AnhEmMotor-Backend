using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Application.Features.PurchaseInvoices.Commands.UpdatePurchaseInvoice
{
    public sealed class UpdatePurchaseInvoiceCommandHandler(
        IPurchaseInvoiceReadRepository readRepository,
        IPurchaseInvoiceUpdateRepository updateRepository,
        IPurchaseInvoiceDeleteRepository deleteRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdatePurchaseInvoiceCommand, Result<PurchaseInvoiceDetailResponse?>>
    {
        public async Task<Result<PurchaseInvoiceDetailResponse?>> Handle(
            UpdatePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            var invoice = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (invoice is null)
            {
                return Error.NotFound($"Không tìm thấy hóa đơn mua hàng có ID {request.Id}.", "Id");
            }

            if (!string.Equals(invoice.Status, "draft", StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest("Không thể chỉnh sửa hóa đơn mua hàng đã duyệt hoặc gửi.", "Status");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return Error.BadRequest("Danh sách sản phẩm trên hóa đơn không được trống.", "Items");
            }

            invoice.InvoiceNumber = request.InvoiceNumber;
            invoice.InvoiceDate = request.InvoiceDate;
            invoice.DueDate = request.DueDate;
            invoice.Note = request.Note;

            var existingItemsDict = invoice.PurchaseInvoiceItems.ToDictionary(x => x.Id);
            var requestItemsDict = request.Items.Where(x => x.Id.HasValue && x.Id > 0).ToDictionary(x => x.Id!.Value);

            // Delete
            var toDelete = invoice.PurchaseInvoiceItems.Where(x => !requestItemsDict.ContainsKey(x.Id)).ToList();
            foreach (var item in toDelete)
            {
                deleteRepository.DeleteItem(item);
                invoice.PurchaseInvoiceItems.Remove(item);
            }

            // Add or Update
            foreach (var itemRequest in request.Items)
            {
                if (itemRequest.Id.HasValue && itemRequest.Id > 0)
                {
                    if (existingItemsDict.TryGetValue(itemRequest.Id.Value, out var existingItem))
                    {
                        existingItem.PurchaseOrderItemId = itemRequest.PurchaseOrderItemId;
                        existingItem.InventoryReceiptItemId = itemRequest.InventoryReceiptItemId;
                        existingItem.ProductVariantId = itemRequest.ProductVariantId;
                        existingItem.ProductVariantColorId = itemRequest.ProductVariantColorId;
                        existingItem.InvoicedQuantity = itemRequest.InvoicedQuantity;
                        existingItem.UnitPrice = itemRequest.UnitPrice;
                        existingItem.TaxRate = itemRequest.TaxRate;
                    }
                }
                else
                {
                    var newItem = new PurchaseInvoiceItem
                    {
                        PurchaseOrderItemId = itemRequest.PurchaseOrderItemId,
                        InventoryReceiptItemId = itemRequest.InventoryReceiptItemId,
                        ProductVariantId = itemRequest.ProductVariantId,
                        ProductVariantColorId = itemRequest.ProductVariantColorId,
                        InvoicedQuantity = itemRequest.InvoicedQuantity,
                        UnitPrice = itemRequest.UnitPrice,
                        TaxRate = itemRequest.TaxRate
                    };
                    invoice.PurchaseInvoiceItems.Add(newItem);
                }
            }

            updateRepository.Update(invoice);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updated = await readRepository.GetByIdWithDetailsAsync(invoice.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<PurchaseInvoiceDetailResponse?>();
        }
    }
}
