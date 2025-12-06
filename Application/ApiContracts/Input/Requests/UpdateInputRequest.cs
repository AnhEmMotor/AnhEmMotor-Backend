namespace Application.ApiContracts.Input.Requests;

public class UpdateInputRequest
{
    public string? Notes { get; set; }

    public int? SupplierId { get; set; }

    public List<UpdateInputInfoRequest> Products { get; set; } = [];
}
