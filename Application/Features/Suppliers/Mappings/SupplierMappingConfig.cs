using Application.ApiContracts.Supplier;
using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using Application.Features.Suppliers.Commands.RestoreManySuppliers;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Application.Features.Suppliers.Commands.UpdateSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplierStatus;
using Domain.Entities;
using Mapster;

namespace Application.Features.Suppliers.Mappings;

public sealed class SupplierMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateSupplierRequest, CreateSupplierCommand>();

        config.NewConfig<CreateSupplierCommand, Supplier>().Map(dest => dest.StatusId, src => "active");

        config.NewConfig<Supplier, SupplierResponse>();

        config.NewConfig<UpdateSupplierRequest, UpdateSupplierCommand>();

        config.NewConfig<UpdateSupplierCommand, Supplier>().IgnoreNullValues(true);

        config.NewConfig<UpdateSupplierStatusRequest, UpdateSupplierStatusCommand>();

        config.NewConfig<UpdateSupplierStatusCommand, Supplier>().IgnoreNullValues(true);

        config.NewConfig<DeleteManySuppliersRequest, DeleteManySuppliersCommand>();

        config.NewConfig<RestoreManySuppliersRequest, RestoreManySuppliersCommand>();

        config.NewConfig<UpdateManySupplierStatusRequest, UpdateManySupplierStatusCommand>();
    }
}