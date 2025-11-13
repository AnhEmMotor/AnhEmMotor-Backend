using Domain.Entities;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace Application.Sieve
{
    public class BrandSieveProcessor(IOptions<SieveOptions> options) : SieveProcessor(options)
    {
        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            mapper.Property<Brand>(b => b.Id)
               .CanSort();

            mapper.Property<Brand>(b => b.Name)
                .CanSort()
                .CanFilter();

            mapper.Property<Brand>(b => b.Description)
                .CanFilter();

            return mapper;
        }
    }
}
