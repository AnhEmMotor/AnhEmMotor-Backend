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
            return Result<VoucherValidateResponse>.Failure(Error.NotFound("Không tìm thấy voucher."));
        decimal total = query.OrderTotal ?? 0;
        if (query.OutputId.HasValue && query.OutputId.Value > 0)
        {
            var output = await outputReadRepository.GetByIdWithDetailsAsync(query.OutputId.Value, ct);
            if (output is null)
                return Result<VoucherValidateResponse>.Failure(Error.NotFound("Không tìm thấy đơn hàng."));
            total = output.Total;
        }
        var today = DateTime.UtcNow.Date;
        if (today < voucher.ValidFrom.Date)
            return Result<VoucherValidateResponse>.Failure(Error.BadRequest("Voucher chưa đến hạn sử dụng."));
        if (today > voucher.ValidTo.Date)
            return Result<VoucherValidateResponse>.Failure(Error.BadRequest("Voucher đã hết hạn."));
        var totalUsed = await voucherUsageRepository.GetTotalUsageCountAsync(voucher.Id, ct);
        if (voucher.TotalUsageLimit > 0 && totalUsed >= voucher.TotalUsageLimit)
            return Result<VoucherValidateResponse>.Failure(Error.BadRequest("Voucher đã hết lượt sử dụng."));
        if (query.OutputId.HasValue && query.OutputId.Value > 0)
        {
            var existingOnOutput = await voucherUsageRepository.GetByVoucherAndOutputAsync(
                voucher.Id,
                query.OutputId.Value,
                ct);
            if (existingOnOutput is not null)
                return Result<VoucherValidateResponse>.Failure(
                    Error.BadRequest("Voucher đã được áp dụng cho đơn hàng này."));
        }
        if (voucher.MinOrderValue > 0 && total < voucher.MinOrderValue)
            return Result<VoucherValidateResponse>.Failure(
                Error.BadRequest($"Đơn hàng tối thiểu {voucher.MinOrderValue:N0} VND."));
        var discountAmount = voucher.DiscountType == DiscountType.Percent
            ? voucher.DiscountValue * total / 100
            : voucher.DiscountValue;
        if (voucher.MaxDiscountAmount > 0 && discountAmount > voucher.MaxDiscountAmount)
            discountAmount = voucher.MaxDiscountAmount.Value;
        discountAmount = Math.Min(discountAmount, total);
        return Result<VoucherValidateResponse>.Success(
            new VoucherValidateResponse { IsValid = true, DiscountAmount = discountAmount, Message = string.Empty });
    }
}
