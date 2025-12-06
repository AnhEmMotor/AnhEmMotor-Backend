namespace Application.ApiContracts.Input.Requests;

public class CreateInputRequest
{
    public string? Notes { get; set; }

    public int? SupplierId { get; set; }

    public List<CreateInputInfoRequest> Products { get; set; } = [];
}
