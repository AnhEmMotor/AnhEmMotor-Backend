using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.DebtPayment.Requests
{
    public class UpdateDebtProofImagesRequest
    {
        [JsonPropertyName("proofImageUrls")]
        public List<string> ProofImageUrls { get; set; } = new();
    }
}
