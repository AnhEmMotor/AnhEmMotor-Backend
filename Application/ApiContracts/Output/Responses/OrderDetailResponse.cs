
namespace Application.ApiContracts.Output.Responses;

public class OrderDetailResponse
{
    public int? Id { get; set; }

    public string? StatusId { get; set; }

    public string? Notes { get; set; }

    public Guid? BuyerId { get; set; }

    public string? BuyerName { get; set; }

    public string? BuyerPhone { get; set; }

    public string? BuyerEmail { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerPhone { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public string? CompletedByUserName { get; set; }

    public decimal? Total { get; set; }
    public int? DepositRatio { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? RemainingAmount { get; set; }

    public List<OutputInfoResponse> Products { get; set; } = [];
}
