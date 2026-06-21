using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.DebtPayment.Requests
{
    public class PaySupplierDebtRequest
    {
        [Required(ErrorMessage = "Số tiền thanh toán là bắt buộc.")]
        [Range(1, double.MaxValue, ErrorMessage = "Số tiền thanh toán phải lớn hơn 0.")]
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}
