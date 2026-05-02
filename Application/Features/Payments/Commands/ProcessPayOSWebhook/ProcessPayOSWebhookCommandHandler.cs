using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using MediatR;
using System;

namespace Application.Features.Payments.Commands.ProcessPayOSWebhook
{
    public sealed class ProcessPayOSWebhookCommandHandler(
        IOutputReadRepository readRepository,
        IOutputUpdateRepository updateRepository,
        IUnitOfWork unitOfWork,
        IPayOSService payosService) : IRequestHandler<ProcessPayOSWebhookCommand, Result>
    {
        public async Task<Result> Handle(ProcessPayOSWebhookCommand request, CancellationToken cancellationToken)
        {
            if(!payosService.VerifyWebhook(request.Data))
            {
                return Result.Failure(Error.BadRequest("Chữ ký PayOS không hợp lệ", "Signature"));
            }

            if(!long.TryParse(request.Data.OrderCode, out var fullOrderCode))
            {
                return Result.Success();
            }

            var orderId = (int)(fullOrderCode / 100000);
            if(orderId == 0)
                orderId = (int)fullOrderCode;

            var order = await readRepository.GetByIdWithDetailsAsync(orderId, cancellationToken).ConfigureAwait(false);
            if(order is null || string.Compare(order.PaymentMethod, PaymentMethod.PayOS) != 0)
            {
                return Result.Success();
            }

            if(string.Compare(order.PaymentStatus, OrderPaymentStatus.Paid) == 0)
            {
                return Result.Success();
            }

            if(request.Data.Amount >= order.Total)
            {
                order.StatusId = OrderStatus.PaidProcessing;
                order.PaymentStatus = OrderPaymentStatus.Paid;
            } else if(request.Data.Amount >= order.DepositAmount)
            {
                order.StatusId = OrderStatus.DepositPaid;
                order.PaymentStatus = OrderPaymentStatus.Partial;
            } else
            {
                return Result.Success();
            }

            order.TransactionId = request.Data.TransactionId;
            order.PaidAmount = request.Data.Amount;
            order.PaidAt = DateTimeOffset.Now;

            updateRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
