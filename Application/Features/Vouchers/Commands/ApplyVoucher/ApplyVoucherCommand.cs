using Application.Common.Models;
using MediatR;

namespace Application.Features.Vouchers.Commands.ApplyVoucher;

/// <summary>
/// Command để áp dụng voucher vào đơn hàng.
/// </summary>
public class ApplyVoucherCommand : IRequest<Result<ApplyVoucherResponse>>
{
	public int VoucherId { get; set; }
	public int OutputId { get; set; }
	public Guid CurrentUserId { get; set; }
}
