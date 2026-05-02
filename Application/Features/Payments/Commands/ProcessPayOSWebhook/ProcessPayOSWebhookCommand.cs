using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Payments.Commands.ProcessPayOSWebhook;

public sealed record ProcessPayOSWebhookCommand(PayOSWebhookData Data) : IRequest<Result>;

public sealed class ProcessPayOSWebhookCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork,
    IPayOSService payosService) : IRequestHandler<ProcessPayOSWebhookCommand, Result>
{
    public async Task<Result> Handle(ProcessPayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!payosService.VerifyWebhook(request.Data))
        {
            return Result.Failure(Error.BadRequest("Chữ ký PayOS không hợp lệ", "Signature"));
        }

        if (!long.TryParse(request.Data.OrderCode, out var fullOrderCode))
        {
            return Result.Success();
        }

        var orderId = (int)(fullOrderCode / 100000);
        if (orderId == 0) orderId = (int)fullOrderCode; // Fallback for legacy orderCodes

        var order = await readRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || order.PaymentMethod != PaymentMethod.PayOS)
        {
            return Result.Success();
        }

        if (order.PaymentStatus == "Paid")
        {
            return Result.Success();
        }

        // PayOS only calls webhook on success (unless configured otherwise)
        if (request.Data.Amount >= order.Total)
        {
            order.StatusId = OrderStatus.PaidProcessing;
            order.PaymentStatus = "Paid";
        }
        else if (request.Data.Amount >= order.DepositAmount)
        {
            order.StatusId = OrderStatus.DepositPaid;
            order.PaymentStatus = "Partial";
        }
        else
        {
            return Result.Success();
        }

        order.TransactionId = request.Data.TransactionId;
        order.PaidAmount = request.Data.Amount;
        order.PaidAt = DateTimeOffset.Now;

        updateRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
