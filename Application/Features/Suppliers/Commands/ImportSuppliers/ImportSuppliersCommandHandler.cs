using Application.Common.Models;
using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Excel;
using Domain.Constants;
using MediatR;
using Microsoft.Extensions.Configuration;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Features.Suppliers.Commands.ImportSuppliers;

public class ImportSuppliersCommandHandler(
    ISupplierInsertRepository repository,
    ISupplierReadRepository supplierReadRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    ISupplierExcelService excelService) : IRequestHandler<ImportSuppliersCommand, Result<ImportSuppliersResult>>
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
        var importRows = excelService.ParseImportRows(fileBytes);
        if (importRows == null)
        {
            return Result<ImportSuppliersResult>.Failure(Error.BadRequest("Excel file does not contain any worksheet."));
        }
        var suppliersToInsert = new List<SupplierEntity>();
        var failedRowsData = new List<(string PartnerTypeId, string Name, string Phone, string Email, string TaxId, string Address, string Notes, string Reason)>(
            );
        var validator = new CreateSupplierCommandValidator();
        foreach (var importRow in importRows)
        {
            var partnerTypeIdRaw = importRow.PartnerTypeId;
            var matchKey = PartnerType.GetKeyFromName(partnerTypeIdRaw);
            var partnerTypeId = !string.IsNullOrEmpty(matchKey) ? matchKey : partnerTypeIdRaw;
            var name = importRow.Name;
            var phone = importRow.Phone;
            var email = importRow.Email;
            var taxId = importRow.TaxIdentificationNumber;
            var address = importRow.Address;
            var notes = importRow.Notes;
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
            if (!string.IsNullOrWhiteSpace(name))
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
            var failedRows = failedRowsData
                .Select(
                    f => new SupplierImportFailedRow(
                        f.PartnerTypeId,
                        f.Name,
                        f.Phone,
                        f.Email,
                        f.TaxId,
                        f.Address,
                        f.Notes,
                        f.Reason))
                .ToList();
            var (file1Bytes, file2Bytes) = excelService.BuildImportErrorReports(failedRows);
            await File.WriteAllBytesAsync(Path.Combine(errorsDir, file1Name), file1Bytes, cancellationToken)
                .ConfigureAwait(false);
            await File.WriteAllBytesAsync(Path.Combine(errorsDir, file2Name), file2Bytes, cancellationToken)
                .ConfigureAwait(false);
            result.ErrorFileUrl = $"/import-errors/{file1Name}";
            result.ErrorFileWithReasonUrl = $"/import-errors/{file2Name}";
        }
        return Result<ImportSuppliersResult>.Success(result);
    }
}
