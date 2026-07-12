using Application.ApiContracts.Voucher.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vouchers.Commands.CreateVoucher;

public class CreateVoucherCommand : IRequest<Result<int>>
{
    public CreateVoucherRequest Request { get; set; } = null!;
}
