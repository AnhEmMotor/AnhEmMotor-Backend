using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Input.Requests
{
    public class CreateInputInfoRequest
    {
        public int? ProductVarientId { get; set; }

        [JsonIgnore]
        public int? ProductId
        {
            get => ProductVarientId;
            set => ProductVarientId = value;
        }

        public int? ProductVarientColorId { get; set; }

        [JsonIgnore]
        public int? ProductVariantColorId
        {
            get => ProductVarientColorId;
            set => ProductVarientColorId = value;
        }

        public int? Count { get; set; }

        public decimal? InputPrice { get; set; }

        public List<VehicleInputRequest>? Vehicles { get; set; }
    }

    public class VehicleInputRequest
    {
        public string VinNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
    }
}
