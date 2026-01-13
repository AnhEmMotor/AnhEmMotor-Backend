using Application.ApiContracts.Supplier.Responses;
using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplierStatus;
using Domain.Entities;
using Mapster;

namespace Application.Features.Suppliers.Mappings;

public sealed class SupplierMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateSupplierCommand, Supplier>().Map(dest => dest.StatusId, src => "active");

        config.NewConfig<Supplier, SupplierResponse>();

        config.NewConfig<UpdateSupplierCommand, Supplier>().IgnoreNullValues(true);

        config.NewConfig<UpdateSupplierStatusCommand, Supplier>().IgnoreNullValues(true);
    }
}