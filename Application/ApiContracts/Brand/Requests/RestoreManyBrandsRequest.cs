
using Application;
using Application.ApiContracts;
using Application.ApiContracts.Brand;

namespace Application.ApiContracts.Brand.Requests
{
    public class RestoreManyBrandsRequest
    {
        public List<int> Ids { get; set; } = [];
    }
}
