using Application.ApiContracts.Voucher.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vouchers.Commands.UpdateVoucher;

public class UpdateVoucherCommand : IRequest<Result<int>>
{
    public UpdateVoucherRequest Request { get; set; } = null!;
}
