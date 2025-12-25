namespace Application.ApiContracts.Input.Responses;

public class InputInfoResponse
{
    public int? Id { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? Count { get; set; }

    public decimal? InputPrice { get; set; }

    public int? RemainingCount { get; set; }
}
