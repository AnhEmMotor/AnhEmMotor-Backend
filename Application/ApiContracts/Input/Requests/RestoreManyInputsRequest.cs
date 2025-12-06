namespace Application.ApiContracts.Input.Requests;

public class RestoreManyInputsRequest
{
    public List<int> Ids { get; set; } = [];
}
