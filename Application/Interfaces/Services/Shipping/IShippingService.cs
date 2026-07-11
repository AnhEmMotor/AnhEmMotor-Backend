using Application.Common.Models;
using Domain.Entities;

namespace Application.Interfaces.Services.Shipping;

public interface IShippingService
{
    /// <summary>
    /// Creates a shipping order with the integrated carrier (e.g., GHN). Returns the Tracking Order Code.
    /// </summary>
    Task<Result<string>> CreateShippingOrderAsync(Output output, CancellationToken cancellationToken = default);

    Task<Result<string>> GetShippingOrderStatusAsync(string orderCode, CancellationToken cancellationToken = default);

    Task<Result<object>> GetProvincesAsync(CancellationToken cancellationToken = default);

    Task<Result<object>> GetWardsAsync(int provinceId, CancellationToken cancellationToken = default);
}
