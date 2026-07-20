using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vouchers.Commands.RemoveVoucher;

/// <summary>
/// Command to remove (un-apply) a previously applied voucher from an order.
/// </summary>
public class RemoveVoucherCommand : IRequest<Result<RemoveVoucherResponse>>
{
    public int OrderVoucherId { get; set; }

    public Guid CurrentUserId { get; set; }

    public RemoveVoucherCommand(int orderVoucherId, Guid currentUserId)
    {
        OrderVoucherId = orderVoucherId;
        CurrentUserId = currentUserId;
    }
}
