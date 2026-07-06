using Application.ApiContracts.Voucher.Requests;
using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Vouchers.Queries.GetVoucherList;

public class GetVouchersQuery : IRequest<Result<PagedResult<VoucherResponse>>>
{
    public GetVouchersRequest Request { get; set; } = null!;
}
