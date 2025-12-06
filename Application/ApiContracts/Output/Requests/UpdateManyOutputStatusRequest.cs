using Application.ApiContracts.Output;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Output.Requests;

public class UpdateManyOutputStatusRequest
{
    public List<int> Ids { get; set; } = [];

    public string? StatusId { get; set; }
}
