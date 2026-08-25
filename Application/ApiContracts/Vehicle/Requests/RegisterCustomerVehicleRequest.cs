using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Vehicle.Requests;

public record RegisterCustomerVehicleRequest
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LicensePlate { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Vin { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EngineNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    public double CurrentOdo { get; set; }
}
