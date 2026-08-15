using Application.ApiContracts.Voucher.Responses;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Client.Vouchers.Queries.GetPersonalVouchers;

public class GetPersonalVouchersQueryHandler : IRequestHandler<GetPersonalVouchersQuery, List<VoucherResponse>>
{
    public Task<List<VoucherResponse>> Handle(GetPersonalVouchersQuery request, CancellationToken cancellationToken)
    {
        // Trả về mock data cho các voucher định danh cá nhân để app mobile có thể bind dữ liệu ở trang chủ
        var mockPersonalVouchers = new List<VoucherResponse>
        {
            new VoucherResponse
            {
                Id = 1,
                Code = "PRIVATE_OIL_20",
                Name = "Giảm 20% Dầu Nhớt",
                Type = VoucherType.Private,
                DiscountType = DiscountType.Percent,
                DiscountValue = 20,
                MaxDiscountAmount = 100000,
                MinOrderValue = 0,
                ValidFrom = DateTime.Now.AddDays(-1),
                ValidTo = DateTime.Now.AddDays(30),
            },
            new VoucherResponse
            {
                Id = 2,
                Code = "PRIVATE_HELMET",
                Name = "Tặng Nón Bảo Hiểm",
                Type = VoucherType.Private,
                DiscountType = DiscountType.Amount,
                DiscountValue = 500000,
                MaxDiscountAmount = 500000,
                MinOrderValue = 1000000,
                ValidFrom = DateTime.Now.AddDays(-1),
                ValidTo = DateTime.Now.AddDays(15),
            },
            new VoucherResponse
            {
                Id = 3,
                Code = "PRIVATE_WASH_50",
                Name = "Giảm 50% Rửa Xe",
                Type = VoucherType.Private,
                DiscountType = DiscountType.Percent,
                DiscountValue = 50,
                MaxDiscountAmount = 50000,
                MinOrderValue = 0,
                ValidFrom = DateTime.Now.AddDays(-5),
                ValidTo = DateTime.Now.AddDays(10),
            },
            new VoucherResponse
            {
                Id = 4,
                Code = "PRIVATE_ACCESSORY_200",
                Name = "Voucher Phụ Kiện 200K",
                Type = VoucherType.Private,
                DiscountType = DiscountType.Amount,
                DiscountValue = 200000,
                MaxDiscountAmount = 200000,
                MinOrderValue = 1500000,
                ValidFrom = DateTime.Now.AddDays(-1),
                ValidTo = DateTime.Now.AddDays(7),
            }
        };

        return Task.FromResult(mockPersonalVouchers);
    }
}
