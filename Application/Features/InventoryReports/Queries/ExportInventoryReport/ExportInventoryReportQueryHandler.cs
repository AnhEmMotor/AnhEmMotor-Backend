using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryOnHand;
using Application.Interfaces.Services.Excel;
using MediatR;

namespace Application.Features.InventoryReports.Queries.ExportInventoryReport
{
    public class ExportInventoryReportQueryHandler(
        IInventoryOnHandReadRepository readRepository,
        IInventoryReportExcelService excelService) : IRequestHandler<ExportInventoryReportQuery, Result<FileStreamResult>>
    {
        public async Task<Result<FileStreamResult>> Handle(
            ExportInventoryReportQuery request,
            CancellationToken cancellationToken)
        {
            var targetMonth = request.Month ?? DateTimeOffset.UtcNow.Month;
            var targetYear = request.Year ?? DateTimeOffset.UtcNow.Year;
            var items = await readRepository.GetInventoryReportSummaryRowsAsync(
                request.SearchTerm,
                request.Month,
                request.Year,
                cancellationToken)
                .ConfigureAwait(false);
            if (items == null || !items.Any())
            {
                return Result<FileStreamResult>.Failure(
                    $"Không có dữ liệu xuất nhập tồn trong tháng {targetMonth}/{targetYear}.");
            }
            var content = excelService.ExportInventoryReport(items, targetMonth, targetYear);
            var fileResult = new FileStreamResult(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Bao_cao_xuat_nhap_ton_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            return Result<FileStreamResult>.Success(fileResult);
        }
    }
}
