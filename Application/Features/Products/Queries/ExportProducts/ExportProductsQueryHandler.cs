using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.Setting;
using Application.Features.Products.Mappings;
using ClosedXML.Excel;
using Domain.Constants;
using MediatR;
using Sieve.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Products.Queries.ExportProducts;

public sealed class ExportProductsQueryHandler(
    IProductReadRepository repository,
    ISettingRepository settingRepository)
    : IRequestHandler<ExportProductsQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        ExportProductsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var alertLevelStr = settings.FirstOrDefault(
            s => string.Equals(s.Key, SettingKeys.InventoryAlertLevel, StringComparison.OrdinalIgnoreCase))?.Value;
        long.TryParse(alertLevelStr, out var alertLevel);

        var sieveModel = request.SieveModel ?? new SieveModel();
        var page = sieveModel.Page ?? 1;
        var pageSize = sieveModel.PageSize ?? 1000;
        var filters = sieveModel.Filters;
        var sorts = sieveModel.Sorts;

        // Extract search from filters if present
        var search = ExtractFilterValue(filters, "search");

        var result = await repository.GetPagedProductsAsync(
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

        var products = result.Items
            .Select(e => ProductMappingConfig.MapProductToDetailForManagerResponseWithAlertLevel(e, alertLevel))
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

        // Headers đầy đủ cho sản phẩm và biến thể (có hình ảnh)
        string[] headers = {
            "STT", "Hình Ảnh", "San Pham", "The Loai", "Thuong Hieu",
            // Thông số kỹ thuật sản phẩm
            "Dong Xe", "Kich Thuoc DxDxS", "Van Do Cuc Dai", "Mo Men Cuc Dai",
            "Dung Tich Dong Co", "Trong Luong", "Chieu Cao Phu Toc", "Khoang Sa Dua",
            "Dung Tich Thang Xang", "He Thong Phanh Trruc Toc", "He Thong Tre Nhon",
            "Lo Xe", "Kieu Dang Truyen Dong", "He Thong Khoi Dong",
            // Thông tin biến thể
            "SKU", "Bien The", "Gia Ban", "Ton Kho", "Trang Thai Ton Kho",
            // Hình ảnh biến thể
            "Hinh Anh Bien The",
            // Kích thước và trọng lượng biến thể
            "Trong Luong BL", "Kich Thuoc BL", "Chieu Cao Phu Toc BL", "Lo Xe BL",
            // Phanh và treo
            "Phanh Truoc BL", "Phanh Sau BL", "Tre Truoc BL", "Tre Sau BL",
            // Động cơ
            "Kieu Dong Co BL", "Cong Suat", "Mo Men"
        };

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

        // Điều chỉnh độ rộng cột
        worksheet.Column(1).Width = 6; // STT
        worksheet.Column(2).Width = 20; // Hình Ảnh
        worksheet.Column(3).Width = 25; // San Pham
        worksheet.Column(4).Width = 20; // The Loai
        worksheet.Column(5).Width = 20; // Thuong Hieu
        worksheet.Column(6).Width = 20; // Dong Xe
        worksheet.Column(7).Width = 20; // Kich Thuoc
        worksheet.Column(8).Width = 15; // Van Do Cuc Dai
        worksheet.Column(9).Width = 15; // Mo Men Cuc Dai
        worksheet.Column(10).Width = 15; // Dung Tich Dong Co
        worksheet.Column(11).Width = 15; // Trong Luong
        worksheet.Column(12).Width = 15; // Chieu Cao Phu Toc
        worksheet.Column(13).Width = 15; // Khoang Sa Dua
        worksheet.Column(14).Width = 15; // Dung Tich Thang Xang
        worksheet.Column(15).Width = 18; // He Thong Phanh Trruc Toc
        worksheet.Column(16).Width = 18; // He Thong Tre Nhon
        worksheet.Column(17).Width = 15; // Lo Xe
        worksheet.Column(18).Width = 18; // Kieu Dang Truyen Dong
        worksheet.Column(19).Width = 18; // He Thong Khoi Dong
        worksheet.Column(20).Width = 20; // SKU
        worksheet.Column(21).Width = 25; // Bien The
        worksheet.Column(22).Width = 15; // Gia Ban
        worksheet.Column(23).Width = 12; // Ton Kho
        worksheet.Column(24).Width = 15; // Trang Thai Ton Kho
        worksheet.Column(25).Width = 30; // Hinh Anh Bien The
        worksheet.Column(26).Width = 15; // Trong Luong BL
        worksheet.Column(27).Width = 15; // Kich Thuoc BL
        worksheet.Column(28).Width = 15; // Chieu Cao Phu Toc BL
        worksheet.Column(29).Width = 12; // Lo Xe BL
        worksheet.Column(30).Width = 18; // Phanh Truoc BL
        worksheet.Column(31).Width = 18; // Phanh Sau BL
        worksheet.Column(32).Width = 18; // Tre Truoc BL
        worksheet.Column(33).Width = 18; // Tre Sau BL
        worksheet.Column(34).Width = 18; // Kieu Dong Co BL
        worksheet.Column(35).Width = 15; // Cong Suat
        worksheet.Column(36).Width = 15; // Mo Men

        int rowIndex = 5;
        int stt = 1;

        foreach (var product in products)
        {
            if (product.Variants != null && product.Variants.Count > 0)
            {
                foreach (var variant in product.Variants)
                {
                    worksheet.Row(rowIndex).Height = 24;

                    // Thông tin cơ bản
                    worksheet.Cell(rowIndex, 1).Value = stt++;
                    worksheet.Cell(rowIndex, 2).Value = product.CoverImageUrl ?? "";
                    worksheet.Cell(rowIndex, 3).Value = product.Name ?? "";
                    worksheet.Cell(rowIndex, 4).Value = product.CategoryName ?? "";
                    worksheet.Cell(rowIndex, 5).Value = product.BrandName ?? "";

                    // Thông số kỹ thuật sản phẩm
                    worksheet.Cell(rowIndex, 6).Value = product.EngineType ?? "";
                    worksheet.Cell(rowIndex, 7).Value = product.Dimensions ?? "";
                    worksheet.Cell(rowIndex, 8).Value = product.MaxPower ?? "";
                    worksheet.Cell(rowIndex, 9).Value = product.MaxTorque ?? "";
                    worksheet.Cell(rowIndex, 10).Value = product.Displacement?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 11).Value = product.Weight?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 12).Value = product.SeatHeight?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 13).Value = product.GroundClearance?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 14).Value = product.FuelCapacity?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 15).Value = product.FrontBrake ?? "";
                    worksheet.Cell(rowIndex, 16).Value = product.RearBrake ?? "";
                    worksheet.Cell(rowIndex, 17).Value = product.TireSize ?? "";
                    worksheet.Cell(rowIndex, 18).Value = product.TransmissionType ?? "";
                    worksheet.Cell(rowIndex, 19).Value = product.StarterSystem ?? "";

                    // Thông tin biến thể
                    worksheet.Cell(rowIndex, 20).Value = variant.SKU ?? "";
                    worksheet.Cell(rowIndex, 21).Value = FormatVariantName(variant);
                    worksheet.Cell(rowIndex, 22).Value = variant.Price?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 23).Value = variant.Stock;
                    worksheet.Cell(rowIndex, 24).Value = variant.InventoryStatus ?? "";

                    // Hình ảnh biến thể
                    var variantImages = string.Join(", ", variant.PhotoCollection.Take(5));
                    worksheet.Cell(rowIndex, 25).Value = variantImages;

                    // Kích thước và trọng lượng biến thể
                    worksheet.Cell(rowIndex, 26).Value = variant.Weight?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 27).Value = variant.Dimensions ?? "";
                    worksheet.Cell(rowIndex, 28).Value = variant.SeatHeight?.ToString("N0") ?? "0";
                    worksheet.Cell(rowIndex, 29).Value = variant.TireSize ?? "";

                    // Phanh và treo
                    worksheet.Cell(rowIndex, 30).Value = variant.FrontBrake ?? "";
                    worksheet.Cell(rowIndex, 31).Value = variant.RearBrake ?? "";
                    worksheet.Cell(rowIndex, 32).Value = variant.FrontSuspension ?? "";
                    worksheet.Cell(rowIndex, 33).Value = variant.RearSuspension ?? "";

                    // Động cơ
                    worksheet.Cell(rowIndex, 34).Value = variant.EngineType ?? "";
                    worksheet.Cell(rowIndex, 35).Value = product.MaxPower ?? "";
                    worksheet.Cell(rowIndex, 36).Value = product.MaxTorque ?? "";

                    ApplyCellStyle(worksheet, rowIndex, 1, 36);
                    rowIndex++;
                }
            }
            else
            {
                worksheet.Row(rowIndex).Height = 24;

                // Thông tin cơ bản
                worksheet.Cell(rowIndex, 1).Value = stt++;
                worksheet.Cell(rowIndex, 2).Value = product.CoverImageUrl ?? "";
                worksheet.Cell(rowIndex, 3).Value = product.Name ?? "";
                worksheet.Cell(rowIndex, 4).Value = product.CategoryName ?? "";
                worksheet.Cell(rowIndex, 5).Value = product.BrandName ?? "";

                // Thông số kỹ thuật sản phẩm
                worksheet.Cell(rowIndex, 6).Value = product.EngineType ?? "";
                worksheet.Cell(rowIndex, 7).Value = product.Dimensions ?? "";
                worksheet.Cell(rowIndex, 8).Value = product.MaxPower ?? "";
                worksheet.Cell(rowIndex, 9).Value = product.MaxTorque ?? "";
                worksheet.Cell(rowIndex, 10).Value = product.Displacement?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 11).Value = product.Weight?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 12).Value = product.SeatHeight?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 13).Value = product.GroundClearance?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 14).Value = product.FuelCapacity?.ToString("N0") ?? "0";
                worksheet.Cell(rowIndex, 15).Value = product.FrontBrake ?? "";
                worksheet.Cell(rowIndex, 16).Value = product.RearBrake ?? "";
                worksheet.Cell(rowIndex, 17).Value = product.TireSize ?? "";
                worksheet.Cell(rowIndex, 18).Value = product.TransmissionType ?? "";
                worksheet.Cell(rowIndex, 19).Value = product.StarterSystem ?? "";

                // Thông tin biến thể (N/A vì không có biến thể)
                worksheet.Cell(rowIndex, 20).Value = product.Stock > 0 ? "Co hang" : "Khong co";
                worksheet.Cell(rowIndex, 21).Value = "N/A";
                worksheet.Cell(rowIndex, 22).Value = "N/A";
                worksheet.Cell(rowIndex, 23).Value = product.Stock;
                worksheet.Cell(rowIndex, 24).Value = product.InventoryStatus ?? "";

                // Hình ảnh biến thể
                worksheet.Cell(rowIndex, 25).Value = "";

                // Các cột còn lại để trống
                for (int col = 26; col <= 36; col++)
                {
                    worksheet.Cell(rowIndex, col).Value = "";
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

        if (!string.IsNullOrWhiteSpace(variant.VersionName))
            parts.Append(variant.VersionName);

        if (!string.IsNullOrWhiteSpace(variant.ColorName))
        {
            if (parts.Length > 0) parts.Append(" - ");
            parts.Append(variant.ColorName);
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

            // Căn giữa các cột số liệu: STT, Giá, Tồn kho, và các thông số kỹ thuật
            if (i == 1 || i == 21 || i == 22 || i == 24 || i == 26 || i == 33 || i == 34)
            {
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
            else
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