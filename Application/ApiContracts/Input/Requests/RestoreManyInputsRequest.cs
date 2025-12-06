using Application.ApiContracts.Input;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Input.Requests;

public class RestoreManyInputsRequest
{
    public List<int> Ids { get; set; } = [];
}
