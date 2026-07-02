using Application.ApiContracts.PurchaseInvoice.Requests;
using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice
{
    public class CreatePurchaseInvoiceCommandHandler(
        IPurchaseInvoiceInsertRepository insertRepository,
        IPurchaseInvoiceReadRepository readRepository,
        IProductVariantReadRepository variantRepository,
        ISupplierReadRepository supplierRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork)
        : IRequestHandler<CreatePurchaseInvoiceCommand, Result<PurchaseInvoiceDetailResponse?>>
    {
        public async Task<Result<PurchaseInvoiceDetailResponse?>> Handle(
            CreatePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Items.Count == 0)
                return Error.BadRequest("Danh sach san pham hoa don khong duoc trong.", "Items");

            var variantIds = request.Items.Select(x => x.ProductVariantId).Distinct().ToList();
            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            var variantDict = variants.ToDictionary(v => v.Id);

            foreach (var item in request.Items)
            {
                if (!variantDict.TryGetValue(item.ProductVariantId, out _))
                    return Error.NotFound($"Khong tim thay bien the san pham co ID {item.ProductVariantId}.", "Items");
                if (item.Quantity <= 0)
                    return Error.BadRequest("So luong phai lon hon 0.", "Items");
                if (item.UnitPrice < 0)
                    return Error.BadRequest("Don gia khong duoc am.", "Items");
            }

            var currentUserId = currentUserContext.GetUserId();
            var now = DateTimeOffset.UtcNow;

            var invoiceNumber = string.IsNullOrWhiteSpace(request.InvoiceNumber)
                ? $"PI-{now.ToUnixTimeSeconds()}"
                : request.InvoiceNumber;

            var invoice = new PurchaseInvoice
            {
                InvoiceNumber = invoiceNumber,
                PurchaseRequestId = request.PurchaseRequestId,
                SupplierId = request.SupplierId,
                SupplierName = request.SupplierName,
                SupplierPhone = request.SupplierPhone,
                SupplierAddress = request.SupplierAddress,
                SupplierTaxCode = request.SupplierTaxCode,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                CustomerAddress = request.CustomerAddress,
                CustomerIdCard = request.CustomerIdCard,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                Status = "draft",
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                CreatedByUserId = currentUserId,
            };

            decimal subTotal = 0m, taxTotal = 0m;

            foreach (var item in request.Items)
            {
                var variant = variantDict[item.ProductVariantId];
                var colorName = item.ProductVariantColorId.HasValue && variant.ProductVariantColors != null
                    ? variant.ProductVariantColors.FirstOrDefault(c => c.Id == item.ProductVariantColorId.Value)?.ColorName
                    : null;

                var lineTotal = item.Quantity * item.UnitPrice;
                var lineTax = Math.Round(lineTotal * item.TaxRate / 100m, 2);
                subTotal += lineTotal;
                taxTotal += lineTax;

                invoice.PurchaseInvoiceItems.Add(new PurchaseInvoiceItem
                {
                    PurchaseRequestItemId = item.PurchaseRequestItemId,
                    ProductVariantId = item.ProductVariantId,
                    ProductVariantColorId = item.ProductVariantColorId,
                    ProductName = variant.Product?.Name,
                    VariantName = variant.VariantName,
                    ColorName = colorName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate,
                    TaxAmount = lineTax,
                    TotalAmount = lineTotal + lineTax,
                });
            }

            invoice.SubTotal = subTotal;
            invoice.TaxAmount = taxTotal;
            invoice.TotalAmount = subTotal + taxTotal;

            insertRepository.Add(invoice);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var created = await readRepository.GetByIdWithItemsAsync(invoice.Id, cancellationToken)
                .ConfigureAwait(false);
            return created!.Adapt<PurchaseInvoiceDetailResponse?>();
        }
    }
}
