using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Quotation;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuotationEntity = Domain.Entities.Quotation;
using Domain.Entities;

namespace Application.Features.Quotations.Commands.CreateQuotation
{
    public sealed class CreateQuotationCommandHandler(
        IQuotationInsertRepository insertRepository,
        IQuotationReadRepository readRepository,
        ISupplierReadRepository supplierRepository,
        IProductVariantReadRepository variantRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateQuotationCommand, Result<QuotationDetailResponse?>>
    {
        public async Task<Result<QuotationDetailResponse?>> Handle(
            CreateQuotationCommand request,
            CancellationToken cancellationToken)
        {
            if (request.SupplierId.HasValue)
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

            var quotation = request.Adapt<QuotationEntity>();
            quotation.Status = "draft";
            quotation.QuotationProductRows = request.Products.Select(p => new QuotationProductRow
            {
                ProductVariantId = int.Parse(p.ProductVariantId!),
                ProductVariantColorId = string.IsNullOrEmpty(p.ProductVarientColorId) ? null : int.Parse(p.ProductVarientColorId),
                QuotePrice = p.QuotePrice
            }).ToList();

            insertRepository.Add(quotation);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var created = await readRepository.GetByIdWithDetailsAsync(quotation.Id, cancellationToken).ConfigureAwait(false);
            return created!.Adapt<QuotationDetailResponse>();
        }
    }
}
