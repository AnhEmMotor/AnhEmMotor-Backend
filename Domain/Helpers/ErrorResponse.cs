namespace Domain.Helpers
{
    public class ErrorResponse
    {
        public List<ErrorDetail> Errors { get; set; } = [];
    }

    public class ErrorDetail
    {
        public string Message { get; set; } = string.Empty;
        public string? Field { get; set; }
    }
}
