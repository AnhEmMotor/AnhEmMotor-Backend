using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Voucher;
using Domain.Enums;
using MediatR;

namespace Application.Features.Vouchers.Queries.ValidateVoucher;

public class VoucherValidateQueryHandler(
    IVoucherReadRepository voucherReadRepository,
    IOutputReadRepository outputReadRepository,
    IVoucherUsageRepository voucherUsageRepository) : IRequestHandler<VoucherValidateQuery, Result<VoucherValidateResponse>>
{
    public async Task<Result<VoucherValidateResponse>> Handle(VoucherValidateQuery query, CancellationToken ct)
    {
        var voucher = await voucherReadRepository.GetByIdAsync(query.VoucherId, ct);
        if (voucher is null)
            return Result<VoucherValidateResponse>.Failure(Error.NotFound("Khong tim thay voucher."));
        var output = await outputReadRepository.GetByIdWithDetailsAsync(query.OutputId, ct);
        if (output is null)
            return Result<VoucherValidateResponse>.Failure(Error.NotFound("Khong tim thay don hang."));
        var today = DateTime.UtcNow.Date;
        if (today < voucher.ValidFrom.Date)
            return Result<VoucherValidateResponse>.Failure(Error.BadRequest("Voucher chua den han."));
        if (today > voucher.ValidTo.Date)
            return Result<VoucherValidateResponse>.Failure(Error.BadRequest("Voucher da het han."));
        var totalUsed = await voucherUsageRepository.GetTotalUsageCountAsync(voucher.Id, ct);
        if (voucher.TotalUsageLimit > 0 && totalUsed >= voucher.TotalUsageLimit)
            return Result<VoucherValidateResponse>.Failure(Error.BadRequest("Voucher da het luot su dung."));
        var existingOnOutput = await voucherUsageRepository.GetByVoucherAndOutputAsync(voucher.Id, query.OutputId, ct);
        if (existingOnOutput is not null)
            return Result<VoucherValidateResponse>.Failure(
                Error.BadRequest("Voucher da duoc ap dung cho don hang nay."));
        var discountAmount = voucher.DiscountType == DiscountType.Percent
            ? voucher.DiscountValue * output.Total / 100
            : voucher.DiscountValue;
        discountAmount = Math.Min(discountAmount, output.Total);
        return Result<VoucherValidateResponse>.Success(
            new VoucherValidateResponse { IsValid = true, DiscountAmount = discountAmount, Message = string.Empty });
    }
}
