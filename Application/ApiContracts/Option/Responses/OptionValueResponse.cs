using System;

namespace Application.ApiContracts.Option.Responses
{
    public class OptionValueResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? SeoTitle { get; set; }

        public string? SeoDescription { get; set; }

        public bool IsActive { get; set; }

        public int ProductCount { get; set; }

        public string? ColorCode { get; set; }
    }
}
