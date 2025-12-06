namespace Application.ApiContracts.Output.Requests;

public class RestoreManyOutputsRequest
{
    public List<int> Ids { get; set; } = [];
}
