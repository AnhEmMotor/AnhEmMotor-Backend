
using Application;
using Application.ApiContracts;
using Application.ApiContracts.Brand;

namespace Application.ApiContracts.Brand.Requests
{
    public class UpdateBrandRequest
    {
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
