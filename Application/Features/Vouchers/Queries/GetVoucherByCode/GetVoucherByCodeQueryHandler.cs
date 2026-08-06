using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Voucher;
using MediatR;
using System.Linq;

namespace Application.Features.Vouchers.Queries.GetVoucherByCode;

public class GetVoucherByCodeQueryHandler(IVoucherReadRepository readRepository) : IRequestHandler<GetVoucherByCodeQuery, Result<VoucherResponse>>
{
    public async Task<Result<VoucherResponse>> Handle(
        GetVoucherByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var voucher = await readRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (voucher == null)
        {
            return Result<VoucherResponse>.Failure(Error.NotFound("Voucher không tồn tại.", "Code"));
        }
        var response = new VoucherResponse
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Name = voucher.Name,
            ApplyFor = voucher.ApplyFor,
            Channel = voucher.Channel,
            Type = voucher.Type,
            DiscountType = voucher.DiscountType,
            DiscountValue = voucher.DiscountValue,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            MinOrderValue = voucher.MinOrderValue,
            ValidFrom = voucher.ValidFrom,
            ValidTo = voucher.ValidTo,
            UsageLimitPerUser = voucher.UsageLimitPerUser,
            TotalUsageLimit = voucher.TotalUsageLimit,
            UsedCount = voucher.UsedCount,
            AssignedCustomerIds = voucher.VoucherLeads?.Select(l => l.LeadId).ToList() ?? new List<int>()
        };
        return Result<VoucherResponse>.Success(response);
    }
}
