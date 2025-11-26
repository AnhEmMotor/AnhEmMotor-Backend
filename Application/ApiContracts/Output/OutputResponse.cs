namespace Application.ApiContracts.Output;

public class OutputResponse
{
    public int? Id { get; set; }

    public string? StatusId { get; set; }

    public string? Notes { get; set; }

    public long? Total { get; set; }

    public List<OutputInfoDto> Products { get; set; } = [];

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
