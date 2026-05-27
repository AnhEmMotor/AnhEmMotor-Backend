using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Quotation;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuotationEntity = Domain.Entities.Quotation;

namespace Application.Features.Quotations.Commands.UpdateQuotation
{
    public sealed class UpdateQuotationCommandHandler(
        IQuotationUpdateRepository updateRepository,
        IQuotationReadRepository readRepository,
        ISupplierReadRepository supplierRepository,
        IProductVariantReadRepository variantRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateQuotationCommand, Result<QuotationDetailResponse?>>
    {
        public async Task<Result<QuotationDetailResponse?>> Handle(
            UpdateQuotationCommand request,
            CancellationToken cancellationToken)
        {
            var quotation = await readRepository.GetByIdWithDetailsAsync(request.Id!.Value, cancellationToken).ConfigureAwait(false);
            if (quotation is null)
            {
                return Error.NotFound($"Yêu cầu báo giá {request.Id} không tồn tại hoặc đã bị xóa.", "Id");
            }

            var currentStatus = quotation.Status?.ToLower();
            if (currentStatus == "approved" || currentStatus == "rejected")
            {
                if (request.SupplierId.HasValue && request.SupplierId.Value != quotation.SupplierId)
                {
                    return Error.BadRequest("Chỉ cho phép cập nhật ghi chú cho báo giá đã được xác nhận hoặc hủy.", "SupplierId");
                }

                var existingRowsList = quotation.QuotationProductRows.ToList();
                if (request.Products.Count != existingRowsList.Count)
                {
                    return Error.BadRequest("Chỉ cho phép cập nhật ghi chú cho báo giá đã được xác nhận hoặc hủy.", "Products");
                }

                foreach (var incomingRow in request.Products)
                {
                    if (!incomingRow.Id.HasValue)
                    {
                        return Error.BadRequest("Chỉ cho phép cập nhật ghi chú cho báo giá đã được xác nhận hoặc hủy.", "Products");
                    }
                    var match = existingRowsList.FirstOrDefault(x => x.Id == incomingRow.Id.Value);
                    if (match is null ||
                        match.ProductVariantId != int.Parse(incomingRow.ProductVariantId!) ||
                        match.ProductVariantColorId != (string.IsNullOrEmpty(incomingRow.ProductVarientColorId) ? null : int.Parse(incomingRow.ProductVarientColorId)) ||
                        match.QuotePrice != incomingRow.QuotePrice)
                    {
                        return Error.BadRequest("Chỉ cho phép cập nhật ghi chú cho báo giá đã được xác nhận hoặc hủy.", "Products");
                    }
                }

                if (request.Notes is not null)
                {
                    quotation.Note = request.Notes;
                }

                updateRepository.Update(quotation);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var updatedDetailed = await readRepository.GetByIdWithDetailsAsync(quotation.Id, cancellationToken).ConfigureAwait(false);
                return updatedDetailed!.Adapt<QuotationDetailResponse>();
            }

            if (request.SupplierId.HasValue && request.SupplierId.Value != quotation.SupplierId)
            {
                var supplier = await supplierRepository.GetByIdAsync(
                    request.SupplierId.Value,
                    cancellationToken,
                    DataFetchMode.ActiveOnly)
                    .ConfigureAwait(false);
                if (supplier is null)
                {
                    return Error.NotFound($"Nhà cung cấp {request.SupplierId} không tồn tại hoặc đã bị xóa.", "SupplierId");
                }
                if (string.Compare(supplier.StatusId, Domain.Constants.SupplierStatus.Active) != 0)
                {
                    return Error.BadRequest($"Nhà cung cấp {supplier.Name} không ở trạng thái 'active'.", "SupplierId");
                }
                quotation.SupplierId = request.SupplierId.Value;
            }

            foreach (var product in request.Products)
            {
                if (int.TryParse(product.ProductVariantId, out var variantId))
                {
                    var variants = await variantRepository.GetByIdAsync(
                        [variantId],
                        cancellationToken,
                        DataFetchMode.ActiveOnly)
                        .ConfigureAwait(false);
                    var variant = variants.FirstOrDefault();
                    if (variant is null)
                    {
                        return Error.BadRequest(
                            $"Biến thể sản phẩm {product.ProductVariantId} không tồn tại hoặc đã bị xóa.",
                            "Products");
                    }

                    if (!string.IsNullOrEmpty(product.ProductVarientColorId) &&
                        int.TryParse(product.ProductVarientColorId, out var colorId))
                    {
                        var colorExists = variant.ProductVariantColors.Any(c => c.Id == colorId);
                        if (!colorExists && variant.ProductVariantColors.Count > 0)
                        {
                            return Error.BadRequest(
                                $"Color ID {colorId} không thuộc biến thể sản phẩm {variantId}.",
                                "Products");
                        }
                    }
                }
            }

            if (request.Notes is not null)
            {
                quotation.Note = request.Notes;
            }

            var existingRows = quotation.QuotationProductRows.ToList();
            var incomingRows = request.Products;

            foreach (var existingRow in existingRows)
            {
                if (!incomingRows.Any(x => x.Id.HasValue && x.Id.Value == existingRow.Id))
                {
                    quotation.QuotationProductRows.Remove(existingRow);
                }
            }

            foreach (var incomingRow in incomingRows)
            {
                if (incomingRow.Id.HasValue && incomingRow.Id.Value > 0)
                {
                    var existingRow = existingRows.FirstOrDefault(x => x.Id == incomingRow.Id.Value);
                    if (existingRow is not null)
                    {
                        existingRow.ProductVariantId = int.Parse(incomingRow.ProductVariantId!);
                        existingRow.ProductVariantColorId = string.IsNullOrEmpty(incomingRow.ProductVarientColorId)
                            ? null
                            : int.Parse(incomingRow.ProductVarientColorId);
                        existingRow.QuotePrice = incomingRow.QuotePrice;
                    }
                }
                else
                {
                    quotation.QuotationProductRows.Add(new QuotationProductRow
                    {
                        ProductVariantId = int.Parse(incomingRow.ProductVariantId!),
                        ProductVariantColorId = string.IsNullOrEmpty(incomingRow.ProductVarientColorId)
                            ? null
                            : int.Parse(incomingRow.ProductVarientColorId),
                        QuotePrice = incomingRow.QuotePrice
                    });
                }
            }

            updateRepository.Update(quotation);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updated = await readRepository.GetByIdWithDetailsAsync(quotation.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<QuotationDetailResponse>();
        }
    }
}
