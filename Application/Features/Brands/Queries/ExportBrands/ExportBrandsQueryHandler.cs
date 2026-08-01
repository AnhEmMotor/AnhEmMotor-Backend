using Application.Common.Models;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Excel;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.ExportBrands;

public class ExportBrandsQueryHandler(IBrandReadRepository repository, IBrandExcelService excelService) : IRequestHandler<ExportBrandsQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(ExportBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await repository.GetFilteredListAsync(
            request.SieveModel ?? new SieveModel(),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var content = excelService.ExportBrands(brands);
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Danh_sach_thuong_hieu.xlsx");
        return Result<FileStreamResult>.Success(fileResult);
    }
}
