namespace Application.ApiContracts.Output;

public class UpdateManyOutputStatusRequest
{
    public List<int> Ids { get; set; } = [];

    public string? StatusId { get; set; }
}
