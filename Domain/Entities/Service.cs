using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Service : BaseEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public int CategoryId { get; set; }

    public ServiceCategory Category { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
