using Application.ApiContracts.Input;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Input.Requests;

public class UpdateInputStatusRequest
{
    public string? StatusId { get; set; }
}
