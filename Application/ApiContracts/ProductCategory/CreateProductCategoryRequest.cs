using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.ProductCategory
{
    public class CreateProductCategoryRequest
    {
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
