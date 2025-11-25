using Application.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product;

public class UpdateManyProductStatusesRequest
{
    public List<int>? Ids { get; set; }

    public string? StatusId { get; set; }
}
