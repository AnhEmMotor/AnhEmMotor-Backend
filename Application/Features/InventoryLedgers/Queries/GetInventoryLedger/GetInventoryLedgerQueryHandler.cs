using Application.ApiContracts.InventoryLedger.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryLedger;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryLedgers.Queries.GetInventoryLedger
{
    public sealed class GetInventoryLedgerQueryHandler(IInventoryLedgerRepository ledgerRepository)
        : IRequestHandler<GetInventoryLedgerQuery, Result<List<InventoryLedgerResponse>>>
    {
        public async Task<Result<List<InventoryLedgerResponse>>> Handle(
            GetInventoryLedgerQuery request,
            CancellationToken cancellationToken)
        {
            var entries = await ledgerRepository.GetAllWithDetailsAsync(cancellationToken).ConfigureAwait(false);

            var query = entries.AsEnumerable();

            // 1. Transaction Type Filter
            if (!string.IsNullOrWhiteSpace(request.Type) && !string.Equals(request.Type, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(request.Type, "IMPORT", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.ImportQty > 0);
                }
                else if (string.Equals(request.Type, "EXPORT", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.ExportQty > 0);
                }
                else if (string.Equals(request.Type, "ADJUST", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.ImportQty == 0 && x.ExportQty == 0);
                }
            }

            // 2. Date Range Filter
            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate <= request.EndDate.Value);
            }

            // 3. Search Query Filter
            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                var search = request.SearchQuery.Trim().ToLower();
                query = query.Where(x =>
                    (!string.IsNullOrEmpty(x.DocumentCode) && x.DocumentCode.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(x.PartnerName) && x.PartnerName.ToLower().Contains(search)) ||
                    (x.ProductVariant?.Product != null && !string.IsNullOrEmpty(x.ProductVariant.Product.Name) && x.ProductVariant.Product.Name.ToLower().Contains(search)) ||
                    (x.ProductVariant != null && !string.IsNullOrEmpty(x.ProductVariant.VariantName) && x.ProductVariant.VariantName.ToLower().Contains(search)) ||
                    (x.ProductVariantColor != null && !string.IsNullOrEmpty(x.ProductVariantColor.ColorName) && x.ProductVariantColor.ColorName.ToLower().Contains(search))
                );
            }

            var response = query.Select(x => new InventoryLedgerResponse
            {
                Id = x.Id,
                Date = x.TransactionDate,
                VoucherCode = x.DocumentCode,
                Type = x.ImportQty > 0 ? "IMPORT" : (x.ExportQty > 0 ? "EXPORT" : "ADJUST"),
                ProductName = x.ProductVariant?.Product?.Name ?? string.Empty,
                VariantName = x.ProductVariant?.VariantName ?? string.Empty,
                ColorName = x.ProductVariantColor?.ColorName,
                Partner = x.PartnerName,
                ImportQty = x.ImportQty,
                ExportQty = x.ExportQty,
                UnitPrice = x.UnitPrice,
                TotalAmount = x.TotalAmount,
                Balance = x.StockAfter
            }).ToList();

            return Result<List<InventoryLedgerResponse>>.Success(response);
        }
    }
}
