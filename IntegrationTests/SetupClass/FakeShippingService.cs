using Application.Common.Models;
using Application.Interfaces.Services.Shipping;
using Application.Interfaces.Services.Shipping.Models;
using Domain.Entities;

namespace IntegrationTests.SetupClass;

public class FakeShippingService : IShippingService
{
    public Task<string?> GetProvinceNameAsync(int provinceId, CancellationToken cancellationToken = default) => Task.FromResult<string?>(
        "Mock Province");

    public Task<string?> GetWardNameAsync(
        int provinceId,
        string wardCode,
        CancellationToken cancellationToken = default) => Task.FromResult<string?>("Mock Ward");

    public Task<Result<string>> GetProvincesAsync(CancellationToken cancellationToken = default) => Task.FromResult(
        Result<string>.Success("{ \"data\": [ { \"_id\": 1, \"name\": \"Mock Province\" } ] }"));

    public Task<Result<string>> GetWardsAsync(int provinceId, CancellationToken cancellationToken = default) => Task.FromResult(
        Result<string>.Success("{ \"data\": [ { \"_id\": 1, \"name\": \"Mock Ward\" } ] }"));

    public Task<Result<decimal>> CalculateShippingFeeAsync(
        CalculateShippingFeeRequest req,
        CancellationToken cancellationToken = default) => Task.FromResult(Result<decimal>.Success(200000m));

    public Task<Result<string>> CreateShippingOrderAsync(Output output, CancellationToken cancellationToken = default) => Task.FromResult(
        Result<string>.Success("Mock"));

    public Task<Result<string>> GetShippingOrderStatusAsync(
        string trackingCode,
        CancellationToken cancellationToken = default) => Task.FromResult(Result<string>.Success("Mock"));

    public Task<Result<bool>> SwitchToReturnOrderAsync(
        string orderCode,
        CancellationToken cancellationToken = default) => Task.FromResult(Result<bool>.Success(true));

    public Task<Result<string>> CreateReturnPickupOrderAsync(
        Output output,
        ReturnRequest returnRequest,
        CancellationToken cancellationToken = default) => Task.FromResult(Result<string>.Success("MockReturn"));
}
