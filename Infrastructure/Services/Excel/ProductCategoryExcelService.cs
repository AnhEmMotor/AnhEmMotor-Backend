using Application.Interfaces.Services.Excel;
using ClosedXML.Excel;
using Domain.Entities;

namespace Infrastructure.Services.Excel;

public class ProductCategoryExcelService : IProductCategoryExcelService
{
    public byte[] ExportProductCategories(IReadOnlyList<ProductCategory> categories, IReadOnlyList<ProductCategory> allCategories)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thể loại sản phẩm");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "DANH SÁCH THỂ LOẠI SẢN PHẨM";
        var titleRange = worksheet.Range("A1:F1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange = worksheet.Range("A2:F2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers = { "STT", "Hình Ảnh", "Tên Thể Loại", "Đường Dẫn (Slug)", "Danh Mục Cha", "Mô Tả" };
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
        worksheet.Column(1).Width = 8;
        worksheet.Column(2).Width = 35;
        worksheet.Column(3).Width = 30;
        worksheet.Column(4).Width = 25;
        worksheet.Column(5).Width = 25;
        worksheet.Column(6).Width = 45;
        var treeOrderedCategories = new List<(ProductCategory Category, int Level, bool IsLastChild)>();
        var rootCategories = categories
            .Where(c => !c.ParentId.HasValue || !categories.Any(p => p.Id == c.ParentId.Value))
            .OrderBy(c => c.Name)
            .ToList();
        foreach (var root in rootCategories)
        {
            treeOrderedCategories.Add((root, 0, false));
            var children = categories
                .Where(c => c.ParentId == root.Id)
                .OrderBy(c => c.Name)
                .ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var isLast = (i == children.Count - 1);
                treeOrderedCategories.Add((child, 1, isLast));
            }
        }
        int rowIndex = 5;
        int stt = 1;
        foreach (var item in treeOrderedCategories)
        {
            var category = item.Category;
            worksheet.Row(rowIndex).Height = 24;
            worksheet.Cell(rowIndex, 1).Value = stt++;
            if (!string.IsNullOrWhiteSpace(category.ImageUrl))
            {
                var cellUrl = worksheet.Cell(rowIndex, 2);
                cellUrl.Value = category.ImageUrl;
                if (category.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                    category.ImageUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    cellUrl.SetHyperlink(new XLHyperlink(category.ImageUrl));
                    cellUrl.Style.Font.FontColor = XLColor.Blue;
                    cellUrl.Style.Font.Underline = XLFontUnderlineValues.Single;
                }
            } else
            {
                worksheet.Cell(rowIndex, 2).Value = "Chưa cấu hình";
            }
            string displayName = category.Name ?? string.Empty;
            var nameCell = worksheet.Cell(rowIndex, 3);
            nameCell.Value = displayName;
            worksheet.Cell(rowIndex, 4).Value = category.Slug ?? string.Empty;
            var parentName = category.ParentId.HasValue
                ? (allCategories.FirstOrDefault(c => c.Id == category.ParentId.Value)?.Name ?? "Không xác định")
                : string.Empty;
            worksheet.Cell(rowIndex, 5).Value = parentName;
            worksheet.Cell(rowIndex, 6).Value = category.Description ?? string.Empty;
            worksheet.Cell(rowIndex, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 2).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 3).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 4).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 5).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 6).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            for (int i = 1; i <= 6; i++)
            {
                var cell = worksheet.Cell(rowIndex, i);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                cell.Style.Font.FontSize = 11;
            }
            rowIndex++;
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
