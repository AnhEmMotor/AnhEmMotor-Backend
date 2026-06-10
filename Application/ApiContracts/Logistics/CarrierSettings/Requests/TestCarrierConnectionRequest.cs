namespace Application.ApiContracts.Logistics.CarrierSettings.Requests;

public class TestCarrierConnectionRequest
{
    // In real integration, backend would call provider ping using current stored settings.
    // Request is empty for now to keep UI simple.
    public string? Note { get; set; }
}

