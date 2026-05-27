
namespace Application.ApiContracts.Vehicle.Responses;

public class VehicleDocumentResponse
{
    public int Id { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
