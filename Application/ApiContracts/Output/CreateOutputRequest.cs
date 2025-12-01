namespace Application.ApiContracts.Output;

public class CreateOutputRequest
{
    public string? Notes { get; set; }

    public List<CreateOutputInfoRequest> Products { get; set; } = [];
}

public class CreateOutputInfoRequest
{
    public int? ProductId { get; set; }

    public short? Count { get; set; }

    public long? Price { get; set; }
}
