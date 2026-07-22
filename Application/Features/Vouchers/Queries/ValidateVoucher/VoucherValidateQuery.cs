using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vouchers.Queries.ValidateVoucher;

public class VoucherValidateQuery(int voucherId, int outputId) : IRequest<Result<VoucherValidateResponse>>
{
    public int VoucherId { get; } = voucherId;

    public int OutputId { get; } = outputId;
}
