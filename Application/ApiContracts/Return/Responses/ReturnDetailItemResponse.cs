using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Return.Responses
{
    public class ReturnDetailItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int Quantity { get; set; }
    }
}
