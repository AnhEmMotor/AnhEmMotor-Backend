using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output.Requests
{
    public class CreateOutputInfoRequest
    {
        public int? ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int? Count { get; set; }
    }
}
