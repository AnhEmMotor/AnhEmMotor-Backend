namespace Application.ApiContracts.Output.Responses;

public class OutputResponse
{
    public int? Id { get; set; }

    public string? StatusId { get; set; }

    public string? Notes { get; set; }

    public Guid? BuyerId { get; set; }

    public string? BuyerName { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public string? CompletedByUserName { get; set; }

    public long? Total { get; set; }

    public List<OutputInfoResponse> Products { get; set; } = [];
}
