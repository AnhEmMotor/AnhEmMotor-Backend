using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Voucher.Requests;

public class ApplyVoucherRequest
{
	[Required]
	public int VoucherId { get; set; }

	[Required]
	public int OutputId { get; set; }
}
