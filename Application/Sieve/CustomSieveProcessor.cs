using Domain.Entities;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;
using System.Reflection;

namespace Application.Sieve;

public class CustomSieveProcessor(IOptions<SieveOptions> options) : SieveProcessor(options)
{
    protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
    {
        var entityTypes = typeof(BaseEntity).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)));

        var method = typeof(CustomSieveProcessor).GetMethod(
            nameof(MapBaseProperties),
            BindingFlags.NonPublic | BindingFlags.Static);

        foreach(var type in entityTypes)
        {
            var genericMethod = method!.MakeGenericMethod(type);
            genericMethod.Invoke(null, [ mapper ]);
        }

        mapper.Property<Brand>(p => p.Id).CanSort();
        mapper.Property<Input>(p => p.Id).CanSort();
        mapper.Property<Output>(p => p.Id).CanSort();
        mapper.Property<Product>(p => p.Id).CanSort();
        mapper.Property<ProductVariant>(p => p.Id).CanSort();
        mapper.Property<Supplier>(p => p.Id).CanSort();

        mapper.Property<Brand>(b => b.Name).CanSort().CanFilter();
        mapper.Property<Brand>(b => b.Description).CanFilter();

        mapper.Property<Input>(i => i.InputDate).CanSort().CanFilter();
        mapper.Property<Input>(i => i.StatusId).CanSort().CanFilter();
        mapper.Property<Input>(i => i.SupplierId).CanSort().CanFilter();
        mapper.Property<Input>(i => i.Notes).CanFilter();

        mapper.Property<Output>(o => o.StatusId).CanSort().CanFilter();
        mapper.Property<Output>(o => o.Notes).CanFilter();

        mapper.Property<ProductCategory>(c => c.Name).CanSort().CanFilter();
        mapper.Property<ProductCategory>(c => c.Description).CanFilter();

        mapper.Property<Supplier>(s => s.Name).CanSort().CanFilter();
        mapper.Property<Supplier>(s => s.Phone).CanFilter();
        mapper.Property<Supplier>(s => s.Email).CanFilter();
        mapper.Property<Supplier>(s => s.StatusId).CanSort().CanFilter();
        mapper.Property<Supplier>(s => s.Address).CanFilter();

        return mapper;
    }

    private static void MapBaseProperties<T>(SievePropertyMapper mapper) where T : BaseEntity
    {
        mapper.Property<T>(x => x.CreatedAt).CanSort().HasName("createdAt");
        mapper.Property<T>(x => x.UpdatedAt).CanSort().HasName("updatedAt");
        mapper.Property<T>(x => x.DeletedAt).CanSort().HasName("deletedAt");
    }
}