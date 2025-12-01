namespace Application.ApiContracts.Input;

public class UpdateInputRequest
{
    public string? Notes { get; set; }

    public int? SupplierId { get; set; }

    public List<UpdateInputInfoRequest> Products { get; set; } = [];
}

public class UpdateInputInfoRequest
{
    public int? Id { get; set; }

    public int? ProductId { get; set; }

    public short? Count { get; set; }

    public long? InputPrice { get; set; }
}
