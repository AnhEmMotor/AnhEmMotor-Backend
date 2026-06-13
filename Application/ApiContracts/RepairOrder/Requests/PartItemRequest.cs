using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.RepairOrder.Requests
{
    public class PartItemRequest
    {
        public int ProductVariantId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
    }
}
