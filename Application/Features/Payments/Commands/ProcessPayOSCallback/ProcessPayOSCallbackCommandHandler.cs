using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using MediatR;
using System;

namespace Application.Features.Payments.Commands.ProcessPayOSCallback
{
    public sealed class ProcessPayOSCallbackCommandHandler(
        IOutputReadRepository readRepository,
        IOutputUpdateRepository updateRepository,
        IUnitOfWork unitOfWork,
        IPayOSService payosService) : IRequestHandler<ProcessPayOSCallbackCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(ProcessPayOSCallbackCommand request, CancellationToken cancellationToken)
        {
            var payosData = await payosService.GetPaymentDetailsAsync(request.OrderCode, cancellationToken)
                .ConfigureAwait(false);
            if (payosData == null)
            {
                return Result<int>.Failure(Error.NotFound("Không tìm thấy thông tin thanh toán từ PayOS", "Payment"));
            }
            var orderId = (int)(request.OrderCode / 100000);
            if (orderId == 0)
                orderId = (int)request.OrderCode;
            var order = await readRepository.GetByIdWithDetailsAsync(orderId, cancellationToken).ConfigureAwait(false);
            if (order is null)
            {
                return Result<int>.Failure(Error.NotFound("Không tìm thấy đơn hàng", "Order"));
            }
            if (string.Compare(payosData.Status, PayOSStatus.Paid) == 0 &&
                string.Compare(order.PaymentStatus, OrderPaymentStatus.Paid) != 0)
            {
                if (payosData.Amount >= order.Total)
                {
                    order.StatusId = OrderStatus.PaidProcessing;
                    order.PaymentStatus = OrderPaymentStatus.Paid;
                } else if (payosData.Amount >= order.DepositAmount)
                {
                    order.StatusId = OrderStatus.DepositPaid;
                    order.PaymentStatus = OrderPaymentStatus.Partial;
                }
                order.PaidAmount = payosData.Amount;
                order.PaidAt = DateTimeOffset.Now;
                updateRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result<int>.Success(order.Id);
            }
            if (string.Compare(payosData.Status, PayOSStatus.Cancelled) == 0)
            {
                order.StatusId = OrderStatus.Cancelled;
                order.PaymentStatus = OrderPaymentStatus.Failed;
                updateRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result<int>.Failure(Error.BadRequest("Giao dịch đã bị hủy", "Payment"));
            }
            if (string.Compare(payosData.Status, PayOSStatus.Paid) == 0)
                return Result<int>.Success(order.Id);
            return Result<int>.Failure(
                Error.BadRequest($"Giao dịch không thành công (Trạng thái: {payosData.Status})", "Payment"));
        }
    }
}
