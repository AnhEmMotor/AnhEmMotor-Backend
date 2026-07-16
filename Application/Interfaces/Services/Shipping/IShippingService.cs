using Application.Common.Models;
using Domain.Entities;

using Application.Interfaces.Services.Shipping.Models;

namespace Application.Interfaces.Services.Shipping;

public interface IShippingService
{
    public Task<Result<decimal>> CalculateShippingFeeAsync(CalculateShippingFeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a shipping order with the integrated carrier (e.g., GHN). Returns the Tracking Order Code.
    /// </summary>
    public Task<Result<string>> CreateShippingOrderAsync(Output output, CancellationToken cancellationToken = default);

    public Task<Result<string>> GetShippingOrderStatusAsync(string orderCode, CancellationToken cancellationToken = default);

    public Task<Result<string>> GetProvincesAsync(CancellationToken cancellationToken = default);

    public Task<Result<string>> GetWardsAsync(int provinceId, CancellationToken cancellationToken = default);

    public Task<string?> GetProvinceNameAsync(int provinceId, CancellationToken cancellationToken = default);
    
    public Task<string?> GetWardNameAsync(int provinceId, string wardCode, CancellationToken cancellationToken = default);
}
