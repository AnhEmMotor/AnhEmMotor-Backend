using System;

namespace Application.ApiContracts.Brand.Responses
{
    public class BrandStatisticsResponse
    {
        public int TotalBrands { get; set; }

        public string? PopularOrigin { get; set; }

        public int PopularOriginCount { get; set; }

        public string? LatestUpdatedBrandName { get; set; }

        public DateTimeOffset? LatestUpdatedAt { get; set; }
    }
}
