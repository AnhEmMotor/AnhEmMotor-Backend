using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Create;

public class ProductOptionRequest
{
    public int Id { get; set; }
    
    public string? Values { get; set; }
}
