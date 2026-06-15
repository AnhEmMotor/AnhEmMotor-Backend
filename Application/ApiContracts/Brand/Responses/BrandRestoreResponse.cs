using System;

namespace Application.ApiContracts.Brand.Responses
{
    public class BrandRestoreResponse
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }
    }
}
