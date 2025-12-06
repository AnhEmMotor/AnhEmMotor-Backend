namespace Application.ApiContracts.Input.Responses;

public class InputInfoResponse
{
    public int? Id { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public short? Count { get; set; }

    public long? InputPrice { get; set; }

    public long? RemainingCount { get; set; }
}
