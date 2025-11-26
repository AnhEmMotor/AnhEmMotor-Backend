namespace Application.ApiContracts.Input;

public class UpdateManyInputStatusRequest
{
    public List<int> Ids { get; set; } = [];

    public string? StatusId { get; set; }
}
