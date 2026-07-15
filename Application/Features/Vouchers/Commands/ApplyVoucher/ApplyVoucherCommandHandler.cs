using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Voucher;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Vouchers.Commands.ApplyVoucher;

public class ApplyVoucherCommandHandler(
    IVoucherReadRepository voucherReadRepository,
    IVoucherUsageRepository voucherUsageRepository,
    IOutputReadRepository outputReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ApplyVoucherCommand, Result<ApplyVoucherResponse>>
{
    public async Task<Result<ApplyVoucherResponse>> Handle(
        ApplyVoucherCommand request,
        CancellationToken cancellationToken)
    {
        var voucher = await voucherReadRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (voucher is null)
            return Result<ApplyVoucherResponse>.Failure(Error.NotFound("Khong tim thay voucher."));

        var output = await outputReadRepository.GetByIdWithDetailsAsync(request.OutputId, cancellationToken);
        if (output is null)
            return Result<ApplyVoucherResponse>.Failure(Error.NotFound("Khong tim thay don hang."));

        var utcNow = DateTimeOffset.UtcNow;

        // Validate dates (ValidFrom/ValidTo are DateTime, compare with local date)
        var today = utcNow.DateTime.Date;
        if (today < voucher.ValidFrom.Date)
            return Result<ApplyVoucherResponse>.Failure(Error.BadRequest("Voucher chua den han."));
        if (today > voucher.ValidTo.Date)
            return Result<ApplyVoucherResponse>.Failure(Error.BadRequest("Voucher da het han."));

        // Total global usage limit
        var totalUsed = await voucherUsageRepository.GetTotalUsageCountAsync(voucher.Id, cancellationToken);
        if (voucher.TotalUsageLimit > 0 && totalUsed >= voucher.TotalUsageLimit)
            return Result<ApplyVoucherResponse>.Failure(Error.BadRequest("Voucher da het luot su dung."));

        // Prevent duplicate application on same output
        var existingOnOutput = await voucherUsageRepository.GetByVoucherAndOutputAsync(voucher.Id, request.OutputId, cancellationToken);
        if (existingOnOutput is not null)
            return Result<ApplyVoucherResponse>.Failure(Error.BadRequest("Voucher da duoc ap dung cho don hang nay."));

        // Per-user usage limit
        var userUsedCount = await voucherUsageRepository.GetUserUsageCountAsync(voucher.Id, request.CurrentUserId, cancellationToken);
        if (voucher.UsageLimitPerUser > 0 && userUsedCount >= voucher.UsageLimitPerUser)
            return Result<ApplyVoucherResponse>.Failure(Error.BadRequest("So lan su dung voucher cua ban da het."));

        // Calculate actual discount based on order total
        var orderTotal = output.Total;
        var discountAmount = voucher.DiscountType == DiscountType.Percent
            ? voucher.DiscountValue * orderTotal / 100
            : voucher.DiscountValue;

        discountAmount = Math.Min(discountAmount, orderTotal);

        var appliedAt = utcNow;
        var orderVoucher = new OrderVoucher
        {
            VoucherId = voucher.Id,
            OutputId = output.Id,
            DiscountApplied = discountAmount,
            AppliedAt = appliedAt,
            AppliedBy = request.CurrentUserId.ToString()
        };

        await voucherUsageRepository.AddAsync(orderVoucher, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ApplyVoucherResponse
        {
            OrderVoucherId = orderVoucher.Id,
            VoucherCode = voucher.Code,
            VoucherName = voucher.Name ?? string.Empty,
            DiscountAmount = discountAmount,
            AppliedAt = appliedAt
        };

        return Result<ApplyVoucherResponse>.Success(response);
    }
}
