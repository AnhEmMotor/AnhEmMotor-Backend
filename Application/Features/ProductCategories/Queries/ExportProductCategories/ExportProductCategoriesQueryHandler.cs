using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using ClosedXML.Excel;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Application.Features.ProductCategories.Queries.ExportProductCategories;

public sealed class ExportProductCategoriesQueryHandler(IProductCategoryReadRepository repository) : IRequestHandler<ExportProductCategoriesQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        ExportProductCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var allCategories = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        string? searchKeyword = null;
        if (!string.IsNullOrWhiteSpace(request.SieveModel?.Filters))
        {
            var match = Regex.Match(request.SieveModel.Filters, @"Name@=(.+?)(?:,|$)");
            if (match.Success)
            {
                searchKeyword = match.Groups[1].Value.Trim();
            }
            else
            {
                match = Regex.Match(request.SieveModel.Filters, @"Name==(.+?)(?:,|$)");
                if (match.Success)
                {
                    searchKeyword = match.Groups[1].Value.Trim();
                }
            }
        }

        List<Domain.Entities.ProductCategory> categories;
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var matchedCategories = allCategories.Where(c => 
                RemoveDiacritics(c.Name ?? "").Contains(RemoveDiacritics(searchKeyword), StringComparison.OrdinalIgnoreCase)
            ).ToList();

            var resultIds = new HashSet<int>();
            foreach (var cat in matchedCategories)
            {
                resultIds.Add(cat.Id);

                // Traverse upwards to root
                var parent = cat;
                while (parent.ParentId.HasValue)
                {
                    var parentId = parent.ParentId.Value;
                    if (!resultIds.Add(parentId)) break;
                    parent = allCategories.FirstOrDefault(c => c.Id == parentId);
                    if (parent == null) break;
                }

                // Traverse downwards to subcategories
                var children = allCategories.Where(c => c.ParentId == cat.Id);
                foreach (var child in children)
                {
                    resultIds.Add(child.Id);
                }
            }

            categories = allCategories.Where(c => resultIds.Contains(c.Id)).ToList();
        }
        else
        {
            categories = allCategories;
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thể loại sản phẩm");

        // Set row heights
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;

        // Title
        worksheet.Cell("A1").Value = "DANH SÁCH THỂ LOẠI SẢN PHẨM";
        var titleRange = worksheet.Range("A1:F1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Date of export
        worksheet.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange = worksheet.Range("A2:F2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Table headers (Matching Brand style colors)
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

        // Set column widths
        worksheet.Column(1).Width = 8;   // STT
        worksheet.Column(2).Width = 35;  // Hình Ảnh
        worksheet.Column(3).Width = 30;  // Tên Thể Loại
        worksheet.Column(4).Width = 25;  // Đường Dẫn (Slug)
        worksheet.Column(5).Width = 25;  // Danh Mục Cha
        worksheet.Column(6).Width = 45;  // Mô Tả

        // Build the hierarchical tree list
        var treeOrderedCategories = new List<(Domain.Entities.ProductCategory Category, int Level, bool IsLastChild)>();
        
        // Root categories in the filtered list: ParentId is null, OR the parent is not present in the filtered list
        var rootCategories = categories
            .Where(c => !c.ParentId.HasValue || !categories.Any(p => p.Id == c.ParentId.Value))
            .OrderBy(c => c.Name)
            .ToList();

        foreach (var root in rootCategories)
        {
            treeOrderedCategories.Add((root, 0, false));

            // Find children of this root in the filtered list
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

        // Data rows
        int rowIndex = 5;
        int stt = 1;

        foreach (var item in treeOrderedCategories)
        {
            var category = item.Category;
            // Set nice row height for data
            worksheet.Row(rowIndex).Height = 24;

            worksheet.Cell(rowIndex, 1).Value = stt++;

            // Image URL with hyperlink
            if (!string.IsNullOrWhiteSpace(category.ImageUrl))
            {
                var cellUrl = worksheet.Cell(rowIndex, 2);
                cellUrl.Value = category.ImageUrl;
                if (category.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) || category.ImageUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    cellUrl.SetHyperlink(new XLHyperlink(category.ImageUrl));
                    cellUrl.Style.Font.FontColor = XLColor.Blue;
                    cellUrl.Style.Font.Underline = XLFontUnderlineValues.Single;
                }
            }
            else
            {
                worksheet.Cell(rowIndex, 2).Value = "Chưa cấu hình";
            }

            // Tên thể loại (Sắp xếp theo thứ tự cha - con gần nhau)
            string displayName = category.Name ?? "";
            
            var nameCell = worksheet.Cell(rowIndex, 3);
            nameCell.Value = displayName;
            
            worksheet.Cell(rowIndex, 4).Value = category.Slug ?? "";

            // Parent category name
            var parentName = category.ParentId.HasValue 
                ? (allCategories.FirstOrDefault(c => c.Id == category.ParentId.Value)?.Name ?? "Không xác định")
                : "";
            worksheet.Cell(rowIndex, 5).Value = parentName;
            
            worksheet.Cell(rowIndex, 6).Value = category.Description ?? "";

            // Alignment
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

            // Row styles & borders
            for (int i = 1; i <= 6; i++)
            {
                var cell = worksheet.Cell(rowIndex, i);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                cell.Style.Font.FontSize = 11;
            }

            rowIndex++;
        }

        // Save to stream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Danh_sach_the_loai.xlsx");

        return Result<FileStreamResult>.Success(fileResult);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd').Replace('Đ', 'D');
    }
}
