using Application.ApiContracts.Payment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using MediatR;
using System;

namespace Application.Features.Payments.Commands.ProcessVNPayIPN
{
    public sealed class ProcessVNPayIPNCommandHandler(
        IOutputReadRepository readRepository,
        IOutputUpdateRepository updateRepository,
        IUnitOfWork unitOfWork,
        IVNPayService vnpayService) : IRequestHandler<ProcessVNPayIPNCommand, Result<VNPayPaymentResponse>>
    {
        public async Task<Result<VNPayPaymentResponse>> Handle(
            ProcessVNPayIPNCommand request,
            CancellationToken cancellationToken)
        {
            var response = vnpayService.PaymentExecute(request.Query);

            if(!int.TryParse(response.OrderId, out var orderId))
            {
                return Error.BadRequest("Mã đơn hàng không hợp lệ", "OrderId");
            }

            var order = await readRepository.GetByIdAsync(orderId, cancellationToken).ConfigureAwait(false);
            if(order is null)
            {
                return Error.NotFound("Không tìm thấy đơn hàng", "Order");
            }

            if(string.Compare(order.PaymentStatus, OrderPaymentStatus.Success) == 0)
            {
                return response;
            }

            decimal expectedAmount = string.Compare(order.StatusId, OrderStatus.WaitingDeposit) == 0
                ? order.DepositAmount
                : order.Total;

            if(response.Amount != expectedAmount)
            {
                return Error.BadRequest(
                    $"Số tiền không khớp. Kỳ vọng: {expectedAmount}, Nhận được: {response.Amount}",
                    "Amount");
            }

            if(response.Success && string.Compare(response.VnPayResponseCode, VNPayResponseCode.Success) == 0)
            {
                order.PaymentStatus = OrderPaymentStatus.Success;
                order.TransactionId = response.TransactionId;
                order.PaidAt = DateTimeOffset.UtcNow;

                if(string.Compare(order.StatusId, OrderStatus.WaitingDeposit) == 0)
                {
                    order.StatusId = OrderStatus.DepositPaid;
                } else if(string.Compare(order.StatusId, OrderStatus.Pending) == 0)
                {
                    order.StatusId = OrderStatus.PaidProcessing;
                }

                updateRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            } else
            {
                order.PaymentStatus = OrderPaymentStatus.Failed;
                updateRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return response;
        }
    }
}
