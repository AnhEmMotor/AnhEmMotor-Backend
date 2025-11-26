namespace Application.ApiContracts.Input;

public class UpdateInputRequest
{
    public DateTimeOffset? InputDate { get; set; }

    public string? Notes { get; set; }

    public string? StatusId { get; set; }

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
