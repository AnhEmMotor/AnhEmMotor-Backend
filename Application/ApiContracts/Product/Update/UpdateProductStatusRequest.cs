using Application.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Update;

public class UpdateProductStatusRequest
{
    public string? StatusId { get; set; }
}
