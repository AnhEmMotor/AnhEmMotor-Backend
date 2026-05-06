using Sieve.Models;

namespace Application.Features.Products.Queries.GetProductsList
{
    public class GetProductsRequest : SieveModel
    {
        public string? CategoryIds { get; set; }

        public string? BrandIds { get; set; }

        public string? OptionValueIds { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }
    }
}
