namespace Application.ApiContracts.Product.Requests;

public class UpdateManyProductStatusesRequest
{
    public List<int>? Ids { get; set; }

    public string? StatusId { get; set; }
}
