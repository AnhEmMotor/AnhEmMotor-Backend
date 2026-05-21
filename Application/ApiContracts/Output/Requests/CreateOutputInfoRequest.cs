using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output.Requests
{
    public class CreateOutputInfoRequest
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
    }
}
