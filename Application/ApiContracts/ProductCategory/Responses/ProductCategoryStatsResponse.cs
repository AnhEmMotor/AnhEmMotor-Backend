using System;

namespace Application.ApiContracts.ProductCategory.Responses
{
    public class ProductCategoryStatsResponse
    {
        public int TotalCategories { get; set; }

        public int ProductCategoriesCount { get; set; }

        public int VehicleTypesCount { get; set; }

        public string? NewProductCategoryName { get; set; }

        public string? NewVehicleTypeName { get; set; }

        public int NewProductCategoriesCount { get; set; }

        public int NewVehicleTypesCount { get; set; }
    }
}
