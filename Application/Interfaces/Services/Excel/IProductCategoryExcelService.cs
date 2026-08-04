using Domain.Entities;

namespace Application.Interfaces.Services.Excel;

public interface IProductCategoryExcelService
{
    public byte[] ExportProductCategories(
        IReadOnlyList<ProductCategory> categories,
        IReadOnlyList<ProductCategory> allCategories);
}
