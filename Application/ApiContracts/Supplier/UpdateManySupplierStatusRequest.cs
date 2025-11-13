using Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Supplier
{
    public class UpdateManySupplierStatusRequest
    {
        [Required]
        public List<int> Ids { get; set; } = [];

        [Required]
        [InList("active, inactive", ErrorMessage = "Status Id must be 'active' or 'inactive'")]
        public string? StatusId { get; set; }
    }
}
