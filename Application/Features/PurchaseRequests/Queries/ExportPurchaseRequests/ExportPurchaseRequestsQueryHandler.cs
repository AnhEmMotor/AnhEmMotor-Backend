using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Repositories.Supplier;
using ClosedXML.Excel;
using MediatR;
using System.IO;

namespace Application.Features.PurchaseRequests.Queries.ExportPurchaseRequests;

public class ExportPurchaseRequestsQueryHandler(
    IPurchaseRequestReadRepository readRepository,
    ISupplierReadRepository supplierReadRepository) : IRequestHandler<ExportPurchaseRequestsQuery, byte[]>
{
    public async Task<byte[]> Handle(ExportPurchaseRequestsQuery request, CancellationToken cancellationToken)
    {
        // For exporting, we set page size to a very large number to get all filtered items
        request.SieveModel.PageSize = 100000;
        request.SieveModel.Page = 1;

        var pagedResult = await readRepository.GetPagedAsync<PurchaseRequestListResponse>(request.SieveModel, Domain.Constants.DataFetchMode.ActiveOnly, cancellationToken).ConfigureAwait(false);
        var requests = pagedResult.Items ?? [];
        var requestIds = requests.Select(r => r.Id).ToList();
        var items = new List<Domain.Entities.PurchaseRequestItem>();
        if (requestIds.Any())
        {
            items = await readRepository.GetItemsByPurchaseRequestIdsAsync(requestIds, cancellationToken).ConfigureAwait(false);
        }

        var suppliers = await supplierReadRepository.GetAllAsync(cancellationToken, Domain.Constants.DataFetchMode.All).ConfigureAwait(false);
        var supplierDict = suppliers.ToDictionary(s => s.Id, s => s.Name ?? string.Empty);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Yêu cầu mua hàng");

        worksheet.Cell("A1").Value = "MẪU NHẬP YÊU CẦU MUA HÀNG";
        var titleRange = worksheet.Range("A1:G1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        worksheet.Cell("A2").Value = "Lưu ý: Mỗi dòng là 1 mặt hàng. Các mặt hàng có chung [Mã phiếu] sẽ được gộp thành 1 phiếu.";
        var subtitleRange = worksheet.Range("A2:G2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#EF5350");
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        string[] headers = { "Mã phiếu", "Ghi chú", "Tên sản phẩm", "Tên biến thể sản phẩm", "Tên biến thể màu sắc của sản phẩm (nếu có)", "Số lượng yêu cầu", "Tên nhà cung cấp" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#EF5350"));
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        int rowIndex = 5;
        foreach (var r in requests)
        {
            var rItems = items.Where(x => x.PurchaseRequestId == r.Id).ToList();
            if (rItems.Count == 0)
            {
                worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                worksheet.Cell(rowIndex, 2).Value = r.Note ?? string.Empty;
                rowIndex++;
            }
            else
            {
                foreach (var item in rItems)
                {
                    worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                    worksheet.Cell(rowIndex, 2).Value = r.Note ?? string.Empty;
                    worksheet.Cell(rowIndex, 3).Value = item.ProductVariant?.Product?.Name ?? string.Empty;
                    worksheet.Cell(rowIndex, 4).Value = item.ProductVariant?.VariantName ?? string.Empty;
                    worksheet.Cell(rowIndex, 5).Value = item.ProductVariantColor?.ColorName ?? string.Empty;
                    worksheet.Cell(rowIndex, 6).Value = item.Quantity;
                    
                    string supplierName = string.Empty;
                    if (item.SupplierId.HasValue && supplierDict.TryGetValue(item.SupplierId.Value, out var sName))
                    {
                        supplierName = sName;
                    }
                    worksheet.Cell(rowIndex, 7).Value = supplierName;
                    rowIndex++;
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 30;
        worksheet.Column(3).Width = 30;
        worksheet.Column(4).Width = 30;
        worksheet.Column(5).Width = 40;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 30;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
