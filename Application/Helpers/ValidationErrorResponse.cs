namespace Application.Helpers
{
    public class ValidationErrorResponse
    {
        public string? Title { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
