namespace Application.ApiContracts.Input.Requests
{
    public class VehicleInputRequest
    {
        public int? Id { get; set; }

        public string VinNumber { get; set; } = string.Empty;

        public string EngineNumber { get; set; } = string.Empty;
    }
}
