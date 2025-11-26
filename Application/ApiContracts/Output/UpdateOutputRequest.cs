namespace Application.ApiContracts.Output;

public class UpdateOutputRequest
{
    public string? StatusId { get; set; }

    public string? Notes { get; set; }

    public List<UpdateOutputInfoRequest> Products { get; set; } = [];
}

public class UpdateOutputInfoRequest
{
    public int? Id { get; set; }

    public int? ProductId { get; set; }

    public short? Count { get; set; }

    public long? Price { get; set; }
}
