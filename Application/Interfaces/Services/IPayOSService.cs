using Application.ApiContracts.Payment.Requests;
using Application.ApiContracts.Payment.Responses;

namespace Application.Interfaces.Services;

public interface IPayOSService
{
    public Task<PayOSPaymentResponse> CreatePaymentAsync(
        PayOSPaymentRequest request,
        CancellationToken cancellationToken);

    public bool VerifyWebhook(PayOSWebhookData data);

    public Task<PayOSData?> GetPaymentDetailsAsync(long orderCode, CancellationToken cancellationToken);
}
