using Application.ApiContracts.Payment.Requests;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Outputs.Queries.GetPaymentLink
{
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
            var order = await readRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
                .ConfigureAwait(false);
            if(order is null)
            {
                return Error.NotFound("Không tìm thấy đơn hàng", "Order");
            }

            if(string.Compare(order.StatusId, OrderStatus.WaitingDeposit) != 0 &&
                string.Compare(order.StatusId, OrderStatus.Pending) != 0)
            {
                return Error.BadRequest("Đơn hàng này không ở trạng thái cần thanh toán", "Order");
            }

            var context = httpContextAccessor.HttpContext;
            if(context is null)
            {
                return Error.Failure("Lỗi hệ thống: Không tìm thấy HttpContext", "System");
            }

            var amountToPay = string.Compare(order.StatusId, OrderStatus.WaitingDeposit) == 0
                ? (order.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0)) * (order.DepositRatio ?? 0) / 100)
                : order.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0));

            var paymentMethod = order.PaymentMethod;
            if(string.IsNullOrWhiteSpace(paymentMethod) || string.Compare(paymentMethod, PaymentMethod.COD) == 0)
            {
                paymentMethod = PaymentMethod.PayOS;
                order.PaymentMethod = paymentMethod;
                updateRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            if(string.Compare(paymentMethod, PaymentMethod.VNPay) == 0)
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

            if(string.Compare(paymentMethod, PaymentMethod.PayOS) == 0)
            {
                try
                {
                    var existingPayment = await payosService.GetPaymentDetailsAsync(order.Id, cancellationToken)
                        .ConfigureAwait(false);
                    if(existingPayment != null &&
                        string.Compare(existingPayment.Status, PayOSStatus.Pending) == 0 &&
                        !string.IsNullOrEmpty(existingPayment.CheckoutUrl))
                    {
                        return existingPayment.CheckoutUrl;
                    }
                } catch
                {
                }

                long orderCode = (long)order.Id * 100000 + (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 100000);

                var payosRequest = new PayOSPaymentRequest
                {
                    OrderId = order.Id,
                    OrderCode = orderCode,
                    Amount = amountToPay,
                    Description = $"ANHEMMOTOR {order.Id}"
                };
                var response = await payosService.CreatePaymentAsync(payosRequest, cancellationToken)
                    .ConfigureAwait(false);
                if(response.ErrorCode == 0)
                {
                    return response.CheckoutUrl;
                }
                return Error.Failure($"Lỗi PayOS: {response.Message}", "PayOS");
            }

            return Error.BadRequest("Phương thức thanh toán không hỗ trợ thanh toán online", "PaymentMethod");
        }
    }
}
