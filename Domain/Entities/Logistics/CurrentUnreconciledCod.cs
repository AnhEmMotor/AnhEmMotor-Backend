using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Logistics;

public class CurrentUnreconciledCod
{
    [Key]
    public int Id { get; set; }

    public decimal Value { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

