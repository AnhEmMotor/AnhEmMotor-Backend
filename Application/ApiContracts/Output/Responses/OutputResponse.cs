namespace Application.ApiContracts.Output.Responses;

public class OutputResponse
{
    public int? Id { get; set; }

    public string? StatusId { get; set; }

    public string? Notes { get; set; }

    public long? Total { get; set; }

    public List<OutputInfoResponse> Products { get; set; } = [];
}
