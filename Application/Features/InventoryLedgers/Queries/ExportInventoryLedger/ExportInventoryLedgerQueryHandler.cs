using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Services.Excel;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.InventoryLedgers.Queries.ExportInventoryLedger
{
    public class ExportInventoryLedgerQueryHandler(
        IInventoryLedgerRepository ledgerRepository,
        IInventoryLedgerExcelService excelService) : IRequestHandler<ExportInventoryLedgerQuery, Result<FileStreamResult>>
    {
        public async Task<Result<FileStreamResult>> Handle(
            ExportInventoryLedgerQuery request,
            CancellationToken cancellationToken)
        {
            var entries = await ledgerRepository.GetAllWithDetailsAsync(cancellationToken).ConfigureAwait(false);
            var query = entries.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(request.Type) &&
                !string.Equals(request.Type, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(request.Type, "IMPORT", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.ImportQty > 0);
                } else if (string.Equals(request.Type, "EXPORT", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.ExportQty > 0);
                } else if (string.Equals(request.Type, "ADJUST", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.ImportQty == 0 && x.ExportQty == 0);
                }
            }
            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate <= request.EndDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                query = query.Where(
                    x => (!string.IsNullOrEmpty(x.DocumentCode) && x.DocumentCode.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(x.PartnerName) && x.PartnerName.ToLower().Contains(search)) ||
                        (x.ProductVariant?.Product != null &&
                            !string.IsNullOrEmpty(x.ProductVariant.Product.Name) &&
                            x.ProductVariant.Product.Name.ToLower().Contains(search)) ||
                        (x.ProductVariant != null &&
                            !string.IsNullOrEmpty(x.ProductVariant.VariantName) &&
                            x.ProductVariant.VariantName.ToLower().Contains(search)) ||
                        (x.ProductVariantColor != null &&
                            !string.IsNullOrEmpty(x.ProductVariantColor.ColorName) &&
                            x.ProductVariantColor.ColorName.ToLower().Contains(search)));
            }
            query = query.OrderByDescending(x => x.TransactionDate);
            var items = query.ToList();
            if (!items.Any())
            {
                return Result<FileStreamResult>.Failure("Không có dữ liệu sổ cái tồn kho trong khoảng thời gian này.");
            }
            var content = excelService.ExportInventoryLedger(items, request.StartDate, request.EndDate);
            var fileResult = new FileStreamResult(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"So_cai_ton_kho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            return Result<FileStreamResult>.Success(fileResult);
        }
    }
}
