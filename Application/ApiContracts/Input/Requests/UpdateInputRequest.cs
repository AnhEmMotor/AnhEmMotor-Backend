using Application.ApiContracts.Input;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Input.Requests;

public class UpdateInputRequest
{
    public string? Notes { get; set; }

    public int? SupplierId { get; set; }

    public List<Requests.UpdateInputInfoRequest> Products { get; set; } = [];
}
