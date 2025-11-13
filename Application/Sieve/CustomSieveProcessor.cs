using Domain.Entities;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace Application.Sieve
{
    public class CustomSieveProcessor(IOptions<SieveOptions> options) : SieveProcessor(options)
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

            mapper.Property<Supplier>(s => s.Id)
               .CanSort();

            mapper.Property<Supplier>(s => s.Name)
                .CanSort()
                .CanFilter();

            mapper.Property<Supplier>(s => s.Phone)
                .CanFilter();

            mapper.Property<Supplier>(s => s.Email)
                .CanFilter();

            mapper.Property<Supplier>(s => s.StatusId)
                .CanSort()
                .CanFilter();

            mapper.Property<Supplier>(s => s.Address)
                .CanFilter();

            return mapper;
        }
    }
}
