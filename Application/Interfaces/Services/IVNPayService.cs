using Application.ApiContracts.Payment.Requests;
using Application.ApiContracts.Payment.Responses;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IVNPayService
{
    public string CreatePaymentUrl(HttpContext context, VNPayPaymentRequest model);

    public VNPayPaymentResponse PaymentExecute(IQueryCollection collections);
}

