using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ProductStatus")]
public class ProductStatus
{
    [Key]
    [Column("Key")]
    public string Key { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = [];
}
