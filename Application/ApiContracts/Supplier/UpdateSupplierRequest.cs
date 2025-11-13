using Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Supplier
{
    public class UpdateSupplierRequest
    {
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        [StringLength(50)]
        [EmailAddress]
        public string? Email { get; set; }

        [InList("active, inactive", ErrorMessage = "Status Id must be 'active' or 'inactive'")]
        public string? StatusId { get; set; }

        public string? Notes { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
    }
}
