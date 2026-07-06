namespace Domain.Constants;

public static class WarrantyClaimStatus
{
	public const int Received = 1;
	public const int AwaitingManufacturer = 2;
	public const int Approved = 3;
	public const int Completed = 5;
	public const int Rejected = 6;

	public static string GetLabel(int status) => status switch
	{
		Received => "Tiếp nhận",
		AwaitingManufacturer => "Chờ hãng thẩm định",
		Approved => "Đã duyệt bồi hoàn",
		Completed => "Hoàn tất",
		Rejected => "Từ chối",
		_ => "Không xác định"
	};

	public static bool IsValid(int status) => status is Received or AwaitingManufacturer or Approved or Completed or Rejected;
}
