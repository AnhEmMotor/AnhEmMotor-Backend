namespace Domain.Helpers
{
    public class OperationResult
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Result { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
