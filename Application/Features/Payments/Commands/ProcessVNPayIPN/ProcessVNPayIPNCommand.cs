using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Payments.Commands.ProcessVNPayIPN;

public sealed record ProcessVNPayIPNCommand(IQueryCollection Query) : IRequest<Result<VNPayPaymentResponse>>;

public sealed class ProcessVNPayIPNCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork,
    IVNPayService vnpayService) : IRequestHandler<ProcessVNPayIPNCommand, Result<VNPayPaymentResponse>>
{
    public async Task<Result<VNPayPaymentResponse>> Handle(ProcessVNPayIPNCommand request, CancellationToken cancellationToken)
    {
        var response = vnpayService.PaymentExecute(request.Query);
        
        if (!int.TryParse(response.OrderId, out var orderId))
        {
            return Error.BadRequest("Mã đơn hàng không hợp lệ", "OrderId");
        }

        var order = await readRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Error.NotFound("Không tìm thấy đơn hàng", "Order");
        }

        // 1. Check if order was already processed (Idempotency)
        if (order.PaymentStatus == "Success")
        {
            return response; // Return current response, no further processing needed
        }

        // 2. Check amount
        decimal expectedAmount = order.StatusId == OrderStatus.WaitingDeposit 
            ? order.DepositAmount 
            : order.Total;

        if (response.Amount != expectedAmount)
        {
            return Error.BadRequest($"Số tiền không khớp. Kỳ vọng: {expectedAmount}, Nhận được: {response.Amount}", "Amount");
        }

        if (response.Success && response.VnPayResponseCode == "00")
        {
            order.PaymentStatus = "Success";
            order.TransactionId = response.TransactionId;
            order.PaidAt = DateTimeOffset.UtcNow;
            
            // Determine next status
            if (order.StatusId == OrderStatus.WaitingDeposit)
            {
                order.StatusId = OrderStatus.DepositPaid;
            }
            else if (order.StatusId == OrderStatus.Pending)
            {
                order.StatusId = OrderStatus.PaidProcessing;
            }

            updateRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            order.PaymentStatus = "Failed";
            updateRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
