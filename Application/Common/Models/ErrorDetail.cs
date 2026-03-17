using System.Text.Json.Serialization;

namespace Application.Common.Models
{
    public class ErrorDetail
    {
        private string? _message; 

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message 
        { 
            get => string.IsNullOrWhiteSpace(_message) ? null : _message; 
            set => _message = value; 
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Field { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }
    }
}
