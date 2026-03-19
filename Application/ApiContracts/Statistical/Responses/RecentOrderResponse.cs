
namespace Application.ApiContracts.Statistical.Responses;

public class RecentOrderResponse
{
    public int Id { get; set; }

    public string? OrderCode { get; set; }

    public string? BuyerName { get; set; }

    public decimal TotalAmount { get; set; }

    public string? StatusId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
