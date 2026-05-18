using System;

namespace Application.ApiContracts.ProductCategory.Responses
{
    public class ProductCategoryStatsResponse
    {
        public int TotalCategories { get; set; }

        public int ProductCategoriesCount { get; set; }

        public string? LatestUpdatedCategoryName { get; set; }

        public DateTimeOffset? LatestUpdatedAt { get; set; }
    }
}
