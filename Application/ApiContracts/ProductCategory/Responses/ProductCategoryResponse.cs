namespace Application.ApiContracts.ProductCategory.Responses
{
    public class ProductCategoryResponse
    {
        public int? Id { get; set; }

        public string? Name { get; set; }
        
        public string? Slug { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }

        public int? ParentId { get; set; }

        public string? Description { get; set; }
        
        public string? CategoryGroup { get; set; }

        public int ProductCount { get; set; }
    }
}
