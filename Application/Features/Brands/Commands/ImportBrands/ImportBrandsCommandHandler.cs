using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using ClosedXML.Excel;
using MediatR;
using Microsoft.Extensions.Configuration;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Commands.ImportBrands;

public class ImportBrandsCommandHandler(
    IBrandInsertRepository repository,
    IBrandReadRepository brandReadRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : IRequestHandler<ImportBrandsCommand, Result<ImportBrandsResult>>
{
    public async Task<Result<ImportBrandsResult>> Handle(
        ImportBrandsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return Result<ImportBrandsResult>.Failure(Error.BadRequest("Không tìm thấy file tải lên."));
        }
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        var fileBytes = memoryStream.ToArray();
        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            return Result<ImportBrandsResult>.Failure(Error.BadRequest("Excel file does not contain any worksheet."));
        }
        var brandsToInsert = new List<BrandEntity>();
        var failedRowsData = new List<(string LogoUrl, string Name, string Origin, string Description, string Reason)>();
        var validator = new CreateBrandCommandValidator();
        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (rowCount < 5)
        {
            return Result<ImportBrandsResult>.Success(new ImportBrandsResult());
        }
        for (int i = 5; i <= rowCount; i++)
        {
            var row = worksheet.Row(i);
            var logoUrl = row.Cell(1).GetString()?.Trim() ?? string.Empty;
            if (logoUrl == "Chưa cấu hình")
                logoUrl = string.Empty;
            var name = row.Cell(2).GetString()?.Trim() ?? string.Empty;
            var origin = row.Cell(3).GetString()?.Trim() ?? string.Empty;
            var description = row.Cell(4).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(origin) &&
                string.IsNullOrWhiteSpace(description))
            {
                continue;
            }
            var createCommand = new CreateBrandCommand
            {
                Name = name,
                Origin = string.IsNullOrWhiteSpace(origin) ? null : origin,
                LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl,
                Description = string.IsNullOrWhiteSpace(description) ? null : description
            };
            var validationResult = validator.Validate(createCommand);
            var rowErrors = new List<string>();
            if (!validationResult.IsValid)
            {
                rowErrors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                if (!rowErrors.Any(
                    e => e.Contains("NotEmpty", StringComparison.OrdinalIgnoreCase) ||
                        e.Contains("empty", StringComparison.OrdinalIgnoreCase)))
                {
                    rowErrors.Add("Thiếu tên thương hiệu.");
                }
            } else
            {
                var existingBrands = await brandReadRepository.GetByNameAsync(name, cancellationToken)
                    .ConfigureAwait(false);
                if (existingBrands.Count != 0 ||
                    brandsToInsert.Any(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    rowErrors.Add("Tên thương hiệu đã tồn tại.");
                }
            }
            if (rowErrors.Count > 0)
            {
                failedRowsData.Add((logoUrl, name, origin, description, string.Join(", ", rowErrors)));
            } else
            {
                brandsToInsert.Add(
                    new BrandEntity
                    {
                        Name = name,
                        Origin = createCommand.Origin,
                        LogoUrl = createCommand.LogoUrl,
                        Description = createCommand.Description
                    });
            }
        }
        if (brandsToInsert.Count > 0)
        {
            foreach (var brand in brandsToInsert)
            {
                repository.Add(brand);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var result = new ImportBrandsResult { SuccessCount = brandsToInsert.Count, FailedCount = failedRowsData.Count };
        if (failedRowsData.Count > 0)
        {
            var customUploadPath = configuration["LocalFileStorage:UploadPath"];
            var wwwrootPath = !string.IsNullOrWhiteSpace(customUploadPath)
                ? (Path.IsPathRooted(customUploadPath)
                    ? customUploadPath
                    : Path.Combine(Directory.GetCurrentDirectory(), customUploadPath))
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var errorsDir = Path.Combine(wwwrootPath, "import-errors");
            if (!Directory.Exists(errorsDir))
                Directory.CreateDirectory(errorsDir);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var file1Name = $"ImportErrors_{timestamp}.xlsx";
            var file2Name = $"ImportErrors_WithReason_{timestamp}.xlsx";
            using (var wb1 = new XLWorkbook())
            {
                var ws1 = wb1.Worksheets.Add("Lỗi nhập");
                ws1.Row(1).Height = 40;
                ws1.Row(2).Height = 20;
                ws1.Row(3).Height = 15;
                ws1.Row(4).Height = 30;
                ws1.Cell("A1").Value = "DANH SÁCH LỖI NHẬP THƯƠNG HIỆU";
                var titleRange1 = ws1.Range("A1:D1");
                titleRange1.Merge();
                titleRange1.Style.Font.Bold = true;
                titleRange1.Style.Font.FontSize = 16;
                titleRange1.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
                titleRange1.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                titleRange1.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws1.Cell("A2").Value = $"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                var subtitleRange1 = ws1.Range("A2:D2");
                subtitleRange1.Merge();
                subtitleRange1.Style.Font.Italic = true;
                subtitleRange1.Style.Font.FontSize = 10;
                subtitleRange1.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                subtitleRange1.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                string[] headers1 = { "Đường Dẫn URL Logo", "Tên Thương Hiệu", "Xuất Xứ", "Mô Tả" };
                for (int i = 0; i < headers1.Length; i++)
                {
                    var cell = ws1.Cell(4, i + 1);
                    cell.Value = headers1[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#EF5350"));
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }
                ws1.Column(1).Width = 35;
                ws1.Column(2).Width = 25;
                ws1.Column(3).Width = 15;
                ws1.Column(4).Width = 45;
                for (int i = 0; i < failedRowsData.Count; i++)
                {
                    ws1.Row(i + 5).Height = 24;
                    ws1.Cell(i + 5, 1).Value = failedRowsData[i].LogoUrl;
                    ws1.Cell(i + 5, 2).Value = failedRowsData[i].Name;
                    ws1.Cell(i + 5, 3).Value = failedRowsData[i].Origin;
                    ws1.Cell(i + 5, 4).Value = failedRowsData[i].Description;
                    ws1.Cell(i + 5, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    ws1.Cell(i + 5, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    ws1.Cell(i + 5, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws1.Cell(i + 5, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = ws1.Cell(i + 5, col);
                        cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                        cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        cell.Style.Font.FontSize = 11;
                    }
                }
                wb1.SaveAs(Path.Combine(errorsDir, file1Name));
            }
            using (var wb2 = new XLWorkbook())
            {
                var ws2 = wb2.Worksheets.Add("Lỗi nhập");
                ws2.Row(1).Height = 40;
                ws2.Row(2).Height = 20;
                ws2.Row(3).Height = 15;
                ws2.Row(4).Height = 30;
                ws2.Cell("A1").Value = "DANH SÁCH LỖI NHẬP THƯƠNG HIỆU (KÈM LÝ DO)";
                var titleRange2 = ws2.Range("A1:E1");
                titleRange2.Merge();
                titleRange2.Style.Font.Bold = true;
                titleRange2.Style.Font.FontSize = 16;
                titleRange2.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
                titleRange2.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                titleRange2.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws2.Cell("A2").Value = $"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                var subtitleRange2 = ws2.Range("A2:E2");
                subtitleRange2.Merge();
                subtitleRange2.Style.Font.Italic = true;
                subtitleRange2.Style.Font.FontSize = 10;
                subtitleRange2.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                subtitleRange2.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                string[] headers2 = { "Đường Dẫn URL Logo", "Tên Thương Hiệu", "Xuất Xứ", "Mô Tả", "Lý Do Lỗi" };
                for (int i = 0; i < headers2.Length; i++)
                {
                    var cell = ws2.Cell(4, i + 1);
                    cell.Value = headers2[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#EF5350"));
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }
                ws2.Column(1).Width = 35;
                ws2.Column(2).Width = 25;
                ws2.Column(3).Width = 15;
                ws2.Column(4).Width = 45;
                ws2.Column(5).Width = 40;
                for (int i = 0; i < failedRowsData.Count; i++)
                {
                    ws2.Row(i + 5).Height = 24;
                    ws2.Cell(i + 5, 1).Value = failedRowsData[i].LogoUrl;
                    ws2.Cell(i + 5, 2).Value = failedRowsData[i].Name;
                    ws2.Cell(i + 5, 3).Value = failedRowsData[i].Origin;
                    ws2.Cell(i + 5, 4).Value = failedRowsData[i].Description;
                    ws2.Cell(i + 5, 5).Value = failedRowsData[i].Reason;
                    ws2.Cell(i + 5, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    ws2.Cell(i + 5, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    ws2.Cell(i + 5, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws2.Cell(i + 5, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    ws2.Cell(i + 5, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    for (int col = 1; col <= 5; col++)
                    {
                        var cell = ws2.Cell(i + 5, col);
                        cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                        cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        cell.Style.Font.FontSize = 11;
                    }
                }
                wb2.SaveAs(Path.Combine(errorsDir, file2Name));
            }
            result.ErrorFileUrl = $"/import-errors/{file1Name}";
            result.ErrorFileWithReasonUrl = $"/import-errors/{file2Name}";
        }
        return Result<ImportBrandsResult>.Success(result);
    }
}
