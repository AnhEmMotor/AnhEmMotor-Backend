using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Application.Interfaces.Services;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice
{
    public sealed class CreatePurchaseInvoiceCommandHandler(
        IPurchaseInvoiceInsertRepository insertRepository,
        IPurchaseInvoiceReadRepository readRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<CreatePurchaseInvoiceCommand, Result<PurchaseInvoiceDetailResponse?>>
    {
        public async Task<Result<PurchaseInvoiceDetailResponse?>> Handle(
            CreatePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return Error.BadRequest("Danh sách sản phẩm trên hóa đơn không được trống.", "Items");
            }

            var currentUserId = currentUserContext.GetUserId();
            var purchaseInvoice = new PurchaseInvoiceEntity
            {
                PurchaseOrderId = request.PurchaseOrderId,
                InvoiceNumber = request.InvoiceNumber,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                Status = "draft",
                Note = request.Note,
                CreatedBy = currentUserId,
                PurchaseInvoiceItems = request.Items.Select(item => new PurchaseInvoiceItem
                {
                    PurchaseOrderItemId = item.PurchaseOrderItemId,
                    InventoryReceiptItemId = item.InventoryReceiptItemId,
                    ProductVariantId = item.ProductVariantId,
                    ProductVariantColorId = item.ProductVariantColorId,
                    InvoicedQuantity = item.InvoicedQuantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate
                }).ToList()
            };

            insertRepository.Add(purchaseInvoice);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var created = await readRepository.GetByIdWithDetailsAsync(purchaseInvoice.Id, cancellationToken)
                .ConfigureAwait(false);

            return created.Adapt<PurchaseInvoiceDetailResponse?>();
        }
    }
}
