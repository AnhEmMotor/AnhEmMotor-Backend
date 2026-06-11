using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.Product;
using ClosedXML.Excel;
using MediatR;
using Sieve.Models;
using System;
using System.Linq;
using System.Text;

namespace Application.Features.Products.Queries.ExportProducts;

public sealed class ExportProductsQueryHandler(IProductReadRepository repository) : IRequestHandler<ExportProductsQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {
        var sieveModel = request.SieveModel ?? new SieveModel();
        var page = sieveModel.Page ?? 1;
        var pageSize = sieveModel.PageSize ?? 1000;
        var filters = sieveModel.Filters;
        var sorts = sieveModel.Sorts;
        var search = ExtractFilterValue(filters, "search");
        var (Items, _, _) = await repository.GetPagedProductsAsync(
            search,
            [],
            [],
            [],
            [],
            null,
            null,
            page,
            pageSize,
            filters,
            sorts,
            cancellationToken)
            .ConfigureAwait(false);
        var products = Items
            .Select(ProductMappingConfig.MapProductToDetailForManagerResponseWithAlertLevel)
            .ToList();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("San pham");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "DANH SÁCH SẢN PHẨM VÀ BIẾN THỂ";
        var titleRange = worksheet.Range("A1:I1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = $"Ngay xuat: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange = worksheet.Range("A2:H2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers =["STT", "Hình Ảnh", "San Pham", "The Loai", "Thuong Hieu", "Dong Xe", "Kich Thuoc DxDxS", "Van Do Cuc Dai", "Mo Men Cuc Dai", "Dung Tich Dong Co", "Trong Luong", "Chieu Cao Phu Toc", "Khoang Sa Dua", "Dung Tich Thang Xang", "He Thong Phanh Trruc Toc", "He Thong Tre Nhon", "Lo Xe", "Kieu Dang Truyen Dong", "He Thong Khoi Dong", "SKU", "Bien The", "Gia Ban", "Ton Kho", "Trang Thai Ton Kho", "Hinh Anh Bien The", "Trong Luong BL", "Kich Thuoc BL", "Chieu Cao Phu Toc BL", "Lo Xe BL", "Phanh Truoc BL", "Phanh Sau BL", "Tre Truoc BL", "Tre Sau BL", "Kieu Dong Co BL", "Cong Suat", "Mo Men"];
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
        worksheet.Column(1).Width = 6;
        worksheet.Column(2).Width = 20;
        worksheet.Column(3).Width = 25;
        worksheet.Column(4).Width = 20;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 20;
        worksheet.Column(8).Width = 15;
        worksheet.Column(9).Width = 15;
        worksheet.Column(10).Width = 15;
        worksheet.Column(11).Width = 15;
        worksheet.Column(12).Width = 15;
        worksheet.Column(13).Width = 15;
        worksheet.Column(14).Width = 15;
        worksheet.Column(15).Width = 18;
        worksheet.Column(16).Width = 18;
        worksheet.Column(17).Width = 15;
        worksheet.Column(18).Width = 18;
        worksheet.Column(19).Width = 18;
        worksheet.Column(20).Width = 20;
        worksheet.Column(21).Width = 25;
        worksheet.Column(22).Width = 15;
        worksheet.Column(23).Width = 12;
        worksheet.Column(24).Width = 15;
        worksheet.Column(25).Width = 30;
        worksheet.Column(26).Width = 15;
        worksheet.Column(27).Width = 15;
        worksheet.Column(28).Width = 15;
        worksheet.Column(29).Width = 12;
        worksheet.Column(30).Width = 18;
        worksheet.Column(31).Width = 18;
        worksheet.Column(32).Width = 18;
        worksheet.Column(33).Width = 18;
        worksheet.Column(34).Width = 18;
        worksheet.Column(35).Width = 15;
        worksheet.Column(36).Width = 15;
        int rowIndex = 5;
        int stt = 1;
        foreach (var product in products)
        {
            if (product.Variants != null && product.Variants.Count > 0)
            {
                foreach (var variant in product.Variants)
                {
                    worksheet.Row(rowIndex).Height = 24;
                    worksheet.Cell(rowIndex, 1).Value = stt++;
                    worksheet.Cell(rowIndex, 2).Value = product.CoverImageUrl ?? string.Empty;
                    worksheet.Cell(rowIndex, 3).Value = product.Name ?? string.Empty;
                    worksheet.Cell(rowIndex, 4).Value = product.CategoryName ?? string.Empty;
                    worksheet.Cell(rowIndex, 5).Value = product.BrandName ?? string.Empty;
                    worksheet.Cell(rowIndex, 6).Value = product.EngineType ?? string.Empty;
                    worksheet.Cell(rowIndex, 7).Value = product.Dimensions ?? string.Empty;
                    worksheet.Cell(rowIndex, 8).Value = product.MaxPower ?? string.Empty;
                    worksheet.Cell(rowIndex, 9).Value = product.MaxTorque ?? string.Empty;
                    worksheet.Cell(rowIndex, 10).Value = product.Displacement?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 11).Value = product.Weight?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 12).Value = product.SeatHeight?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 13).Value = product.GroundClearance?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 14).Value = product.FuelCapacity?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 15).Value = product.FrontBrake ?? string.Empty;
                    worksheet.Cell(rowIndex, 16).Value = product.RearBrake ?? string.Empty;
                    worksheet.Cell(rowIndex, 17).Value = product.TireSize ?? string.Empty;
                    worksheet.Cell(rowIndex, 18).Value = product.TransmissionType ?? string.Empty;
                    worksheet.Cell(rowIndex, 19).Value = product.StarterSystem ?? string.Empty;
                    worksheet.Cell(rowIndex, 20).Value = variant.SKU ?? string.Empty;
                    worksheet.Cell(rowIndex, 21).Value = FormatVariantName(variant);
                    worksheet.Cell(rowIndex, 22).Value = variant.Price?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 23).Value = 0;
                    worksheet.Cell(rowIndex, 24).Value = string.Empty;
                    var variantImages = string.Join(", ", variant.PhotoCollection.Take(5));
                    worksheet.Cell(rowIndex, 25).Value = variantImages;
                    worksheet.Cell(rowIndex, 26).Value = variant.Weight?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 27).Value = variant.Dimensions ?? string.Empty;
                    worksheet.Cell(rowIndex, 28).Value = variant.SeatHeight?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 29).Value = variant.TireSize ?? string.Empty;
                    worksheet.Cell(rowIndex, 30).Value = variant.FrontBrake ?? string.Empty;
                    worksheet.Cell(rowIndex, 31).Value = variant.RearBrake ?? string.Empty;
                    worksheet.Cell(rowIndex, 32).Value = variant.FrontSuspension ?? string.Empty;
                    worksheet.Cell(rowIndex, 33).Value = variant.RearSuspension ?? string.Empty;
                    worksheet.Cell(rowIndex, 34).Value = variant.EngineType ?? string.Empty;
                    worksheet.Cell(rowIndex, 35).Value = product.MaxPower ?? string.Empty;
                    worksheet.Cell(rowIndex, 36).Value = product.MaxTorque ?? string.Empty;
                    ApplyCellStyle(worksheet, rowIndex, 1, 36);
                    rowIndex++;
                }
            } else
            {
                worksheet.Row(rowIndex).Height = 24;
                worksheet.Cell(rowIndex, 1).Value = stt++;
                worksheet.Cell(rowIndex, 2).Value = product.CoverImageUrl ?? string.Empty;
                worksheet.Cell(rowIndex, 3).Value = product.Name ?? string.Empty;
                worksheet.Cell(rowIndex, 4).Value = product.CategoryName ?? string.Empty;
                worksheet.Cell(rowIndex, 5).Value = product.BrandName ?? string.Empty;
                worksheet.Cell(rowIndex, 6).Value = product.EngineType ?? string.Empty;
                worksheet.Cell(rowIndex, 7).Value = product.Dimensions ?? string.Empty;
                worksheet.Cell(rowIndex, 8).Value = product.MaxPower ?? string.Empty;
                worksheet.Cell(rowIndex, 9).Value = product.MaxTorque ?? string.Empty;
                worksheet.Cell(rowIndex, 10).Value = product.Displacement?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 11).Value = product.Weight?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 12).Value = product.SeatHeight?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 13).Value = product.GroundClearance?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 14).Value = product.FuelCapacity?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 15).Value = product.FrontBrake ?? string.Empty;
                worksheet.Cell(rowIndex, 16).Value = product.RearBrake ?? string.Empty;
                worksheet.Cell(rowIndex, 17).Value = product.TireSize ?? string.Empty;
                worksheet.Cell(rowIndex, 18).Value = product.TransmissionType ?? string.Empty;
                worksheet.Cell(rowIndex, 19).Value = product.StarterSystem ?? string.Empty;
                worksheet.Cell(rowIndex, 20).Value = "N/A";
                worksheet.Cell(rowIndex, 21).Value = "N/A";
                worksheet.Cell(rowIndex, 22).Value = "N/A";
                worksheet.Cell(rowIndex, 23).Value = 0;
                worksheet.Cell(rowIndex, 24).Value = string.Empty;
                worksheet.Cell(rowIndex, 25).Value = string.Empty;
                for (int col = 26; col <= 36; col++)
                {
                    worksheet.Cell(rowIndex, col).Value = string.Empty;
                }
                ApplyCellStyle(worksheet, rowIndex, 1, 36);
                rowIndex++;
            }
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Danh_sach_san_pham.xlsx");
        return Result<FileStreamResult>.Success(fileResult);
    }

    private static string FormatVariantName(ProductVariantDetailForManagerResponse variant)
    {
        var parts = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(variant.VariantName))
            parts.Append(variant.VariantName);
        var colorNames = variant.Colors
            .Select(color => color.ColorName)
            .Where(colorName => !string.IsNullOrWhiteSpace(colorName))
            .ToList();
        if (colorNames.Count > 0)
        {
            if (parts.Length > 0)
                parts.Append(" - ");
            parts.Append(string.Join(", ", colorNames));
        }
        return parts.Length > 0 ? parts.ToString() : "Co ban";
    }

    private static void ApplyCellStyle(IXLWorksheet worksheet, int rowIndex, int startCol, int endCol)
    {
        for (int i = startCol; i <= endCol; i++)
        {
            var cell = worksheet.Cell(rowIndex, i);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            cell.Style.Font.FontSize = 11;
            if (i == 1 || i == 21 || i == 22 || i == 24 || i == 26 || i == 33 || i == 34)
            {
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            } else
            {
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }
    }

    private static string? ExtractFilterValue(string? filters, string key)
    {
        if (string.IsNullOrWhiteSpace(filters))
        {
            return null;
        }
        var parts = filters.Split(',');
        foreach (var part in parts)
        {
            var keyValue = part.Split(['=', '@', '!', '<', '>'], 2);
            if (keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                var value = keyValue[1].Trim();
                return value.TrimStart('=', '@', '!', '<', '>', '*');
            }
        }
        return null;
    }
}
