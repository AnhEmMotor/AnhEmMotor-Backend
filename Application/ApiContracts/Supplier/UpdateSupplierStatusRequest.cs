using Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Supplier
{
    public class UpdateSupplierStatusRequest
    {
        [Required]
        [InList("active, inactive", ErrorMessage = "Status Id must be 'active' or 'inactive'")]
        public string? StatusId { get; set; }
    }
}
