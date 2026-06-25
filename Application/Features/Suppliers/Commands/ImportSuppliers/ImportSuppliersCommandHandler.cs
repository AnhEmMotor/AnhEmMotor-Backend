using Application.Common.Models;
using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using ClosedXML.Excel;
using Domain.Constants;
using MediatR;
using Microsoft.Extensions.Configuration;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Features.Suppliers.Commands.ImportSuppliers;

public class ImportSuppliersCommandHandler(
    ISupplierInsertRepository repository,
    ISupplierReadRepository supplierReadRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : IRequestHandler<ImportSuppliersCommand, Result<ImportSuppliersResult>>
{
    public async Task<Result<ImportSuppliersResult>> Handle(
        ImportSuppliersCommand request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return Result<ImportSuppliersResult>.Failure(Error.BadRequest("Không tìm thấy file tải lên."));
        }
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        var fileBytes = memoryStream.ToArray();
        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            return Result<ImportSuppliersResult>.Failure(Error.BadRequest("Excel file does not contain any worksheet."));
        }
        var suppliersToInsert = new List<SupplierEntity>();
        var failedRowsData = new List<(string PartnerTypeId, string Name, string Phone, string Email, string TaxId, string Address, string Notes, string Reason)>(
            );
        var validator = new CreateSupplierCommandValidator();
        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (rowCount < 5)
        {
            return Result<ImportSuppliersResult>.Success(new ImportSuppliersResult());
        }
        for (int i = 5; i <= rowCount; i++)
        {
            var row = worksheet.Row(i);
            var partnerTypeIdRaw = row.Cell(1).GetString()?.Trim() ?? string.Empty;
            var matchKey = PartnerType.GetKeyFromName(partnerTypeIdRaw);
            var partnerTypeId = !string.IsNullOrEmpty(matchKey) ? matchKey : partnerTypeIdRaw;
            var name = row.Cell(2).GetString()?.Trim() ?? string.Empty;
            var phone = row.Cell(3).GetString()?.Trim() ?? string.Empty;
            var email = row.Cell(4).GetString()?.Trim() ?? string.Empty;
            var taxId = row.Cell(5).GetString()?.Trim() ?? string.Empty;
            var address = row.Cell(6).GetString()?.Trim() ?? string.Empty;
            var notes = row.Cell(7).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(phone) &&
                string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(address))
            {
                continue;
            }
            var createCommand = new CreateSupplierCommand
            {
                Name = name,
                Phone = phone,
                Email = email,
                Address = address,
                TaxIdentificationNumber = taxId,
                Notes = notes,
                PartnerTypeId = partnerTypeId
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
                    rowErrors.Add("Thiếu tên đối tác.");
                }
            } else
            {
                var nameExists = await supplierReadRepository.IsNameExistsAsync(name, null, cancellationToken)
                    .ConfigureAwait(false);
                if (nameExists ||
                    suppliersToInsert.Any(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    rowErrors.Add("Tên đối tác đã tồn tại.");
                }
            }
            if (rowErrors.Count > 0)
            {
                failedRowsData.Add(
                    (partnerTypeIdRaw, name, phone, email, taxId, address, notes, string.Join(", ", rowErrors)));
            } else
            {
                suppliersToInsert.Add(
                    new SupplierEntity
                    {
                        Name = name,
                        Phone = createCommand.Phone,
                        Email = createCommand.Email,
                        Address = createCommand.Address,
                        TaxIdentificationNumber = createCommand.TaxIdentificationNumber,
                        Notes = createCommand.Notes,
                        PartnerTypeId = createCommand.PartnerTypeId,
                        StatusId = "active"
                    });
            }
        }
        if (suppliersToInsert.Count > 0)
        {
            foreach (var supplier in suppliersToInsert)
            {
                repository.Add(supplier);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var result = new ImportSuppliersResult
        {
            SuccessCount = suppliersToInsert.Count,
            FailedCount = failedRowsData.Count
        };
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
            var file1Name = $"ImportErrors_Supplier_{timestamp}.xlsx";
            var file2Name = $"ImportErrors_Supplier_WithReason_{timestamp}.xlsx";
            using (var wb1 = new XLWorkbook())
            {
                var ws1 = wb1.Worksheets.Add("Lỗi nhập");
                ws1.Row(1).Height = 40;
                ws1.Row(2).Height = 20;
                ws1.Row(3).Height = 15;
                ws1.Row(4).Height = 30;
                ws1.Cell("A1").Value = "DANH SÁCH LỖI NHẬP ĐỐI TÁC";
                var titleRange1 = ws1.Range("A1:G1");
                titleRange1.Merge();
                titleRange1.Style.Font.Bold = true;
                titleRange1.Style.Font.FontSize = 16;
                titleRange1.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
                titleRange1.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                titleRange1.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws1.Cell("A2").Value = $"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                var subtitleRange1 = ws1.Range("A2:G2");
                subtitleRange1.Merge();
                subtitleRange1.Style.Font.Italic = true;
                subtitleRange1.Style.Font.FontSize = 10;
                subtitleRange1.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                subtitleRange1.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                string[] headers1 =
                {
                    "Loại đối tác",
                    "Tên đối tác",
                    "Điện thoại",
                    "Email",
                    "Mã số thuế",
                    "Địa chỉ",
                    "Ghi chú"
                };
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
                ws1.Column(1).Width = 15;
                ws1.Column(2).Width = 25;
                ws1.Column(3).Width = 15;
                ws1.Column(4).Width = 25;
                ws1.Column(5).Width = 15;
                ws1.Column(6).Width = 40;
                ws1.Column(7).Width = 30;
                var typeValidation1 = ws1.Range($"A5:A{Math.Max(5, failedRowsData.Count + 4)}").CreateDataValidation();
                typeValidation1.AllowedValues = XLAllowedValues.List;
                typeValidation1.List(PartnerType.ExcelValidationList);
                typeValidation1.ErrorStyle = XLErrorStyle.Stop;
                typeValidation1.ErrorTitle = "Lỗi nhập liệu";
                typeValidation1.ErrorMessage = "Vui lòng chọn loại đối tác từ danh sách thả xuống.";
                for (int i = 0; i < failedRowsData.Count; i++)
                {
                    ws1.Row(i + 5).Height = 24;
                    ws1.Cell(i + 5, 1).Value = failedRowsData[i].PartnerTypeId;
                    ws1.Cell(i + 5, 2).Value = failedRowsData[i].Name;
                    ws1.Cell(i + 5, 3).Value = failedRowsData[i].Phone;
                    ws1.Cell(i + 5, 4).Value = failedRowsData[i].Email;
                    ws1.Cell(i + 5, 5).Value = failedRowsData[i].TaxId;
                    ws1.Cell(i + 5, 6).Value = failedRowsData[i].Address;
                    ws1.Cell(i + 5, 7).Value = failedRowsData[i].Notes;
                    for (int col = 1; col <= 7; col++)
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
                ws2.Cell("A1").Value = "DANH SÁCH LỖI NHẬP ĐỐI TÁC (KÈM LÝ DO)";
                var titleRange2 = ws2.Range("A1:H1");
                titleRange2.Merge();
                titleRange2.Style.Font.Bold = true;
                titleRange2.Style.Font.FontSize = 16;
                titleRange2.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
                titleRange2.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                titleRange2.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws2.Cell("A2").Value = $"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                var subtitleRange2 = ws2.Range("A2:H2");
                subtitleRange2.Merge();
                subtitleRange2.Style.Font.Italic = true;
                subtitleRange2.Style.Font.FontSize = 10;
                subtitleRange2.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                subtitleRange2.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                string[] headers2 =
                {
                    "Loại đối tác",
                    "Tên đối tác",
                    "Điện thoại",
                    "Email",
                    "Mã số thuế",
                    "Địa chỉ",
                    "Ghi chú",
                    "Lý Do Lỗi"
                };
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
                ws2.Column(1).Width = 15;
                ws2.Column(2).Width = 25;
                ws2.Column(3).Width = 15;
                ws2.Column(4).Width = 25;
                ws2.Column(5).Width = 15;
                ws2.Column(6).Width = 40;
                ws2.Column(7).Width = 30;
                ws2.Column(8).Width = 40;
                var typeValidation2 = ws2.Range($"A5:A{Math.Max(5, failedRowsData.Count + 4)}").CreateDataValidation();
                typeValidation2.AllowedValues = XLAllowedValues.List;
                typeValidation2.List(PartnerType.ExcelValidationList);
                typeValidation2.ErrorStyle = XLErrorStyle.Stop;
                typeValidation2.ErrorTitle = "Lỗi nhập liệu";
                typeValidation2.ErrorMessage = "Vui lòng chọn loại đối tác từ danh sách thả xuống.";
                for (int i = 0; i < failedRowsData.Count; i++)
                {
                    ws2.Row(i + 5).Height = 24;
                    ws2.Cell(i + 5, 1).Value = failedRowsData[i].PartnerTypeId;
                    ws2.Cell(i + 5, 2).Value = failedRowsData[i].Name;
                    ws2.Cell(i + 5, 3).Value = failedRowsData[i].Phone;
                    ws2.Cell(i + 5, 4).Value = failedRowsData[i].Email;
                    ws2.Cell(i + 5, 5).Value = failedRowsData[i].TaxId;
                    ws2.Cell(i + 5, 6).Value = failedRowsData[i].Address;
                    ws2.Cell(i + 5, 7).Value = failedRowsData[i].Notes;
                    ws2.Cell(i + 5, 8).Value = failedRowsData[i].Reason;
                    for (int col = 1; col <= 8; col++)
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
        return Result<ImportSuppliersResult>.Success(result);
    }
}
