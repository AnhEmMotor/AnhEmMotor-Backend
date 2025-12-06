using Application.ApiContracts.Output;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Output.Requests;

public class UpdateOutputStatusRequest
{
    public string? StatusId { get; set; }
}
