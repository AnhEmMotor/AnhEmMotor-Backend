using Application.ApiContracts.Input;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Input.Requests;

public class UpdateManyInputStatusRequest
{
    public List<int> Ids { get; set; } = [];

    public string? StatusId { get; set; }
}
