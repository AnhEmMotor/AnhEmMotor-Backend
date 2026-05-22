using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output.Requests
{
    public class UpdateOutputInfoRequest
    {
        public int? Id { get; set; }

        public int? ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int? Count { get; set; }
    }
}
