namespace Application.Common.Models
{
    public class ErrorDetail
    {
        public string Message { get; set; } = string.Empty;
        public string? Field { get; set; }
        public string? Id { get; set; }
    }
}
