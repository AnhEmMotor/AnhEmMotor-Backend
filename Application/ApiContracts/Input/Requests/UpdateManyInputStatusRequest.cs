namespace Application.ApiContracts.Input.Requests;

public class UpdateManyInputStatusRequest
{
    public List<int> Ids { get; set; } = [];

    public string? StatusId { get; set; }
}
