using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Product
{
    public class ProductFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public List<string> StatusIds { get; set; } = [];
    }
}
