namespace Application.ApiContracts.Technology.Responses
{
    public class TechnologyResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? DefaultTitle { get; set; }

        public string? DefaultDescription { get; set; }

        public string? DefaultImageUrl { get; set; }

        public int? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public int? BrandId { get; set; }
    }
}
