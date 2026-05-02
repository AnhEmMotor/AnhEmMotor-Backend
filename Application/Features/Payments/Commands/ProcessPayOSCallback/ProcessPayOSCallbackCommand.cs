using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Payments.Commands.ProcessPayOSCallback;

public sealed record ProcessPayOSCallbackCommand(long OrderCode) : IRequest<Result<int>>;

public sealed class ProcessPayOSCallbackCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork,
    IPayOSService payosService) : IRequestHandler<ProcessPayOSCallbackCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ProcessPayOSCallbackCommand request, CancellationToken cancellationToken)
    {
        var payosData = await payosService.GetPaymentDetailsAsync(request.OrderCode);
        if (payosData == null)
        {
            return Result<int>.Failure(Error.NotFound("Không tìm thấy thông tin thanh toán từ PayOS", "Payment"));
        }

        // Parse original orderId from the suffixed orderCode
        var orderId = (int)(request.OrderCode / 100000);
        if (orderId == 0) orderId = (int)request.OrderCode; // Fallback for legacy orderCodes
        var order = await readRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Result<int>.Failure(Error.NotFound("Không tìm thấy đơn hàng", "Order"));
        }

        if (payosData.Status == "PAID" && order.PaymentStatus != "Paid")
        {
            if (payosData.Amount >= order.Total)
            {
                order.StatusId = OrderStatus.PaidProcessing;
                order.PaymentStatus = "Paid";
            }
            else if (payosData.Amount >= order.DepositAmount)
            {
                order.StatusId = OrderStatus.DepositPaid;
                order.PaymentStatus = "Partial";
            }

            order.PaidAmount = payosData.Amount;
            order.PaidAt = DateTimeOffset.Now;

            updateRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(order.Id);
        }
        
        if (payosData.Status == "CANCELLED")
        {
            order.StatusId = OrderStatus.Cancelled;
            order.PaymentStatus = "Failed";
            order.Notes = (order.Notes ?? "") + "\n[System] Khách hàng đã hủy thanh toán trên cổng PayOS.";
            updateRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Failure(Error.BadRequest("Giao dịch đã bị hủy", "Payment"));
        }

        if (payosData.Status == "PAID") return Result<int>.Success(order.Id);

        return Result<int>.Failure(Error.BadRequest($"Giao dịch không thành công (Trạng thái: {payosData.Status})", "Payment"));
    }
}
