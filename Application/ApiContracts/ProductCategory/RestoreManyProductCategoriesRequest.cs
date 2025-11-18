using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.ProductCategory
{
    public class RestoreManyProductCategoriesRequest
    {
        [MinLength(1)]
        public List<int> Ids { get; set; } = [];
    }
}
