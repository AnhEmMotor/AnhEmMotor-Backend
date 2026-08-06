using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Excel;
using MediatR;
using Microsoft.Extensions.Configuration;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Commands.ImportBrands;

public class ImportBrandsCommandHandler(
    IBrandInsertRepository repository,
    IBrandReadRepository brandReadRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    IBrandExcelService excelService) : IRequestHandler<ImportBrandsCommand, Result<ImportBrandsResult>>
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
        var importRows = excelService.ParseImportRows(fileBytes);
        var brandsToInsert = new List<BrandEntity>();
        var failedRowsData = new List<(string LogoUrl, string Name, string Origin, string Description, string Reason)>();
        var validator = new CreateBrandCommandValidator();
        foreach (var importRow in importRows)
        {
            var logoUrl = importRow.LogoUrl;
            var name = importRow.Name;
            var origin = importRow.Origin;
            var description = importRow.Description;
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
            var failedRows = failedRowsData
                .Select(f => new BrandImportFailedRow(f.LogoUrl, f.Name, f.Origin, f.Description, f.Reason))
                .ToList();
            var (file1Bytes, file2Bytes) = excelService.BuildImportErrorReports(failedRows);
            await File.WriteAllBytesAsync(Path.Combine(errorsDir, file1Name), file1Bytes, cancellationToken)
                .ConfigureAwait(false);
            await File.WriteAllBytesAsync(Path.Combine(errorsDir, file2Name), file2Bytes, cancellationToken)
                .ConfigureAwait(false);
            result.ErrorFileUrl = $"/import-errors/{file1Name}";
            result.ErrorFileWithReasonUrl = $"/import-errors/{file2Name}";
        }
        return Result<ImportBrandsResult>.Success(result);
    }
}
