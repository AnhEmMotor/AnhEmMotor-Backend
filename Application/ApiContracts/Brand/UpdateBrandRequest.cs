using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Brand
{
    public class UpdateBrandRequest
    {
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
