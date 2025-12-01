namespace Application.ApiContracts.Input;

public class CreateInputRequest
{
    public string? Notes { get; set; }

    public int? SupplierId { get; set; }

    public List<CreateInputInfoRequest> Products { get; set; } = [];
}

public class CreateInputInfoRequest
{
    public int? ProductId { get; set; }

    public short? Count { get; set; }

    public long? InputPrice { get; set; }
}
