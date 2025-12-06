using Application.ApiContracts.Input;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Input.Requests;

public class CreateInputRequest
{
    public string? Notes { get; set; }

    public int? SupplierId { get; set; }

    public List<Requests.CreateInputInfoRequest> Products { get; set; } = [];
}
