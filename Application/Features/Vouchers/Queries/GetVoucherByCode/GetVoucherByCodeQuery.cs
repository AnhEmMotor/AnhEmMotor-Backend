using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vouchers.Queries.GetVoucherByCode;

public class GetVoucherByCodeQuery(string code) : IRequest<Result<VoucherResponse>>
{
    public string Code { get; set; } = code;
}
