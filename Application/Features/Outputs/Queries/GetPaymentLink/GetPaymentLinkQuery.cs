using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Outputs.Queries.GetPaymentLink;

public sealed record GetPaymentLinkQuery(int OrderId) : IRequest<Result<string>>;

public sealed class GetPaymentLinkQueryHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork,
    IVNPayService vnpayService,
    IPayOSService payosService,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetPaymentLinkQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetPaymentLinkQuery request, CancellationToken cancellationToken)
    {
        var order = await readRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Error.NotFound("Không tìm thấy đơn hàng", "Order");
        }

        if (order.StatusId != OrderStatus.WaitingDeposit && order.StatusId != OrderStatus.Pending)
        {
            return Error.BadRequest("Đơn hàng này không ở trạng thái cần thanh toán", "Order");
        }

        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return Error.Failure("Lỗi hệ thống: Không tìm thấy HttpContext", "System");
        }

        var amountToPay = order.StatusId == OrderStatus.WaitingDeposit 
            ? (order.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0)) * (order.DepositRatio ?? 0) / 100)
            : order.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0));

        var paymentMethod = order.PaymentMethod;
        if (string.IsNullOrWhiteSpace(paymentMethod) || paymentMethod == PaymentMethod.COD)
        {
            // If user wants to pay online but order is COD or null, default to PayOS
            paymentMethod = PaymentMethod.PayOS;
            order.PaymentMethod = paymentMethod;
            updateRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (paymentMethod == PaymentMethod.VNPay)
        {
            var vnpayRequest = new VNPayPaymentRequest
            {
                OrderId = order.Id,
                OrderCode = order.Id.ToString(), 
                Amount = amountToPay,
                Description = $"Thanh toan don hang {order.Id}",
                CreatedDate = DateTime.Now
            };
            return vnpayService.CreatePaymentUrl(context, vnpayRequest);
        }

        if (paymentMethod == PaymentMethod.PayOS)
        {
            // 1. Try to reuse an existing PENDING payment link with the original Order ID
            try 
            {
                var existingPayment = await payosService.GetPaymentDetailsAsync(order.Id);
                if (existingPayment != null && existingPayment.Status == "PENDING" && !string.IsNullOrEmpty(existingPayment.CheckoutUrl))
                {
                    return existingPayment.CheckoutUrl;
                }
            }
            catch { /* Ignore error and move to fallback */ }

            // 2. Fallback: Generate a unique orderCode by appending a timestamp suffix
            // This is used when the original ID is already 'Expired' or 'Cancelled' in PayOS
            long orderCode = (long)order.Id * 100000 + (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 100000);

            var payosRequest = new PayOSPaymentRequest
            {
                OrderId = order.Id,
                OrderCode = orderCode,
                Amount = amountToPay,
                Description = $"ANHEMMOTOR {order.Id}"
            };
            var response = await payosService.CreatePaymentAsync(payosRequest);
            if (response.ErrorCode == 0)
            {
                return response.CheckoutUrl;
            }
            return Error.Failure($"Lỗi PayOS: {response.Message}", "PayOS");
        }

        return Error.BadRequest("Phương thức thanh toán không hỗ trợ thanh toán online", "PaymentMethod");
    }
}
