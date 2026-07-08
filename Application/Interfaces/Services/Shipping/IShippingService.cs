using Application.Common.Models;
using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Services.Shipping;

public interface IShippingService
{
    /// <summary>
    /// Creates a shipping order with the integrated carrier (e.g., GHTK).
    /// </summary>
    Task<Result> CreateShippingOrderAsync(Output output, CancellationToken cancellationToken = default);
}
