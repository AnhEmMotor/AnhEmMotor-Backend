using Application;
using Application.ApiContracts;
using Application.ApiContracts.Brand;

namespace Application.ApiContracts.Brand.Responses
{
    public class BrandResponse
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
