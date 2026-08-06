using Application.ApiContracts.Product.Responses;

namespace Application.Interfaces.Services.Excel;

public interface IProductExcelService
{
    public byte[] ExportProducts(IReadOnlyList<ProductDetailForManagerResponse> products);
}
