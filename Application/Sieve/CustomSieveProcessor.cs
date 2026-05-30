using Application.ApiContracts.Permission.Responses;
using Application.ApiContracts.Supplier.Responses;
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
        foreach (var type in entityTypes)
        {
            var genericMethod = method!.MakeGenericMethod(type);
            genericMethod.Invoke(null, [mapper]);
        }
        mapper.Property<Brand>(p => p.Id).CanSort().CanFilter();
        mapper.Property<PurchaseRequest>(p => p.Id).CanSort().CanFilter();
        mapper.Property<InventoryReceipt>(p => p.Id).CanSort().CanFilter();
        mapper.Property<Output>(p => p.Id).CanSort().CanFilter();
        mapper.Property<Product>(p => p.Id).CanSort().CanFilter();
        mapper.Property<Product>(p => p.Name).CanSort().CanFilter();
        mapper.Property<Product>(p => p.BrandId).CanFilter();
        mapper.Property<Product>(p => p.CategoryId).CanFilter().HasName("category_id");
        mapper.Property<Product>(p => p.Brand!.Name).CanFilter().HasName("brand");
        mapper.Property<Product>(p => p.StatusId).CanFilter();
        mapper.Property<Product>(p => p.StdDot).CanFilter();
        mapper.Property<Product>(p => p.StdEce).CanFilter();
        mapper.Property<Product>(p => p.StdSnell).CanFilter();
        mapper.Property<Product>(p => p.StdJis).CanFilter();
        mapper.Property<Product>(p => p.StdDot).CanFilter().HasName("SafetyStandard");
        mapper.Property<News>(p => p.Id).CanSort().CanFilter();
        mapper.Property<News>(p => p.Title).CanSort().CanFilter();
        mapper.Property<News>(p => p.CategoryId).CanFilter();
        mapper.Property<Banner>(p => p.Id).CanSort().CanFilter();
        mapper.Property<Banner>(p => p.Title).CanSort().CanFilter();
        mapper.Property<ProductVariant>(p => p.Id).CanSort().CanFilter();
        mapper.Property<Supplier>(p => p.Id).CanSort().CanFilter();
        mapper.Property<Brand>(b => b.Name).CanSort().CanFilter();
        mapper.Property<Brand>(b => b.Origin).CanSort().CanFilter();
        mapper.Property<Brand>(b => b.Description).CanFilter();
        mapper.Property<InventoryReceipt>(i => i.InventoryReceiptDate).CanSort().CanFilter();
        mapper.Property<InventoryReceipt>(i => i.StatusId).CanSort().CanFilter();
        mapper.Property<InventoryReceipt>(i => i.PurchaseRequestId).CanSort().CanFilter();
        mapper.Property<InventoryReceipt>(i => i.Notes).CanFilter();
        mapper.Property<Output>(o => o.StatusId).CanSort().CanFilter();
        mapper.Property<Output>(o => o.Notes).CanFilter();
        mapper.Property<ProductCategory>(c => c.Id).CanSort().CanFilter();
        mapper.Property<ProductCategory>(c => c.Name).CanSort().CanFilter();
        mapper.Property<ProductCategory>(c => c.Description).CanFilter();
        mapper.Property<ProductCategory>(c => c.ManagementType).CanSort().CanFilter().HasName("managementType");
        mapper.Property<Supplier>(s => s.Name).CanSort().CanFilter();
        mapper.Property<Supplier>(s => s.Phone).CanFilter();
        mapper.Property<Supplier>(s => s.Email).CanFilter();
        mapper.Property<Supplier>(s => s.StatusId).CanSort().CanFilter();
        mapper.Property<Supplier>(s => s.Address).CanFilter();
        mapper.Property<Supplier>(s => s.PartnerTypeId).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.Id).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.Name).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.Phone).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.Email).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.Address).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.StatusId).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.CreatedAt).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.UpdatedAt).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.DeletedAt).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.TotalInventoryReceipt).CanSort().CanFilter();
        mapper.Property<SupplierWithTotalInventoryReceiptResponse>(s => s.PartnerTypeId).CanSort().CanFilter();
        mapper.Property<ApplicationUser>(p => p.Id).CanSort().CanFilter();
        mapper.Property<ApplicationUser>(p => p.UserName).CanSort().CanFilter();
        mapper.Property<ApplicationUser>(p => p.FullName).CanSort().CanFilter();
        mapper.Property<ApplicationUser>(p => p.Email).CanSort().CanFilter();
        mapper.Property<ApplicationUser>(p => p.PhoneNumber).CanSort().CanFilter();
        mapper.Property<ApplicationUser>(p => p.Status).CanSort().CanFilter();
        mapper.Property<ApplicationUser>(p => p.DeletedAt).CanSort().CanFilter();
        mapper.Property<ApplicationRole>(p => p.Id).CanSort().CanFilter();
        mapper.Property<ApplicationRole>(p => p.Name).CanSort().CanFilter();
        mapper.Property<RoleSelectResponse>(p => p.ID).CanSort().CanFilter();
        mapper.Property<RoleSelectResponse>(p => p.Name).CanSort().CanFilter();
        mapper.Property<Vehicle>(v => v.Id).CanSort().CanFilter();
        mapper.Property<Vehicle>(v => v.VinNumber).CanSort().CanFilter();
        mapper.Property<Vehicle>(v => v.EngineNumber).CanSort().CanFilter();
        mapper.Property<Vehicle>(v => v.LicensePlate).CanSort().CanFilter();
        mapper.Property<Vehicle>(v => v.PurchaseDate).CanSort().CanFilter();
        mapper.Property<Vehicle>(v => v.Lead!.FullName).CanSort().CanFilter().HasName("FullName");
        mapper.Property<Vehicle>(v => v.Lead!.PhoneNumber).CanSort().CanFilter().HasName("PhoneNumber");
        mapper.Property<Quotation>(p => p.Id).CanSort().CanFilter();
        mapper.Property<Quotation>(p => p.Status).CanSort().CanFilter();
        mapper.Property<Quotation>(p => p.SupplierId).CanSort().CanFilter();
        mapper.Property<Quotation>(p => p.Supplier!.Name).CanFilter().HasName("SupplierName");
        mapper.Property<Lead>(l => l.Id).CanSort().CanFilter();
        mapper.Property<Lead>(l => l.FullName).CanSort().CanFilter();
        mapper.Property<Lead>(l => l.PhoneNumber).CanSort().CanFilter();
        mapper.Property<Lead>(l => l.Tier).CanSort().CanFilter();
        mapper.Property<Lead>(l => l.Points).CanSort().CanFilter();
        return mapper;
    }

    public IQueryable<Product> SafetyStandard(IQueryable<Product> source, string op, string[] values)
    {
        var val = values[0].ToLower();
        return val switch
        {
            "dot" => source.Where(p => p.StdDot),
            "ece" => source.Where(p => p.StdEce),
            "snell" => source.Where(p => p.StdSnell),
            "jis" => source.Where(p => p.StdJis),
            _ => source
        };
    }

    public IQueryable<Lead> Search(IQueryable<Lead> source, string op, string[] values)
    {
        var term = values[0];
        return source.Where(l => l.FullName.Contains(term) || l.PhoneNumber.Contains(term));
    }

    private static void MapBaseProperties<T>(SievePropertyMapper mapper) where T : BaseEntity
    {
        mapper.Property<T>(x => x.CreatedAt).CanSort().HasName("createdAt");
        mapper.Property<T>(x => x.UpdatedAt).CanSort().HasName("updatedAt");
        mapper.Property<T>(x => x.DeletedAt).CanSort().HasName("deletedAt");
    }
}
