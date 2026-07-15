using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Voucher;
using MediatR;

namespace Application.Features.Vouchers.Commands.RemoveVoucher;

public class RemoveVoucherCommandHandler(
    IVoucherUsageRepository voucherUsageRepository,
    IVoucherReadRepository voucherReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveVoucherCommand, Result<RemoveVoucherResponse>>
{
    public async Task<Result<RemoveVoucherResponse>> Handle(RemoveVoucherCommand request, CancellationToken cancellationToken)
    {
        // In a real scenario we would fetch OrderVoucher with related Voucher+Output for more context.
        // The repository currently exposes Add/Count/Get only. We extend it below with Remove.
        var orderVoucher = await voucherUsageRepository.GetByIdAsync(request.OrderVoucherId, cancellationToken);
        if (orderVoucher is null)
            return Result<RemoveVoucherResponse>.Failure(Error.NotFound("Khong tim thay voucher da ap dung.", "OrderVoucherId"));

        var amount = orderVoucher.DiscountApplied;
        voucherUsageRepository.Remove(orderVoucher);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RemoveVoucherResponse>.Success(new RemoveVoucherResponse
        {
            OrderVoucherId = orderVoucher.Id,
            RefundedAmount = amount
        });
    }
}
