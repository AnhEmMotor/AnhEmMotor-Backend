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
        var entityTypes = typeof(BaseEntity).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)));

        var method = typeof(CustomSieveProcessor).GetMethod(
            nameof(MapBaseProperties),
            BindingFlags.NonPublic | BindingFlags.Static);

        foreach (var type in entityTypes)
        {
            var genericMethod = method!.MakeGenericMethod(type);
            genericMethod.Invoke(null, [mapper]);
        }

        mapper.Property<Brand>(p => p.Id).CanSort();

        mapper.Property<Product>(p => p.Id)
            .CanSort();

        mapper.Property<Brand>(b => b.Name)
            .CanSort()
            .CanFilter();

        mapper.Property<Brand>(b => b.Description)
            .CanFilter();

        mapper.Property<ProductCategory>(c => c.Name)
            .CanSort()
            .CanFilter();

        mapper.Property<ProductCategory>(c => c.Description)
            .CanFilter();

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

    private static void MapBaseProperties<T>(SievePropertyMapper mapper) where T : BaseEntity
    {
        mapper.Property<T>(x => x.CreatedAt).CanSort().HasName("createdAt");

        mapper.Property<T>(x => x.UpdatedAt).CanSort().HasName("updatedAt");

        mapper.Property<T>(x => x.DeletedAt).CanSort().HasName("deletedAt");

    }
}