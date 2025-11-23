using Application.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Supplier
{
    public class UpdateSupplierStatusRequest
    {
        public string? StatusId { get; set; }
    }
}
