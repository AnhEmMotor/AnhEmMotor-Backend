using Application.ApiContracts.Input;
using Application.Features.Inputs.Commands.CreateInput;
using Domain.Entities;
using Mapster;

namespace Application.Features.Inputs.Mappings;

public sealed class InputMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateInputRequest, CreateInputCommand>();
        config.NewConfig<CreateInputInfoRequest, CreateInputProductCommand>();

        config.NewConfig<CreateInputCommand, Input>();
        config.NewConfig<CreateInputProductCommand, InputInfo>();

        config.NewConfig<Input, InputResponse>()
            .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
            .Map(dest => dest.TotalPayable, src => src.InputInfos != null
                ? src.InputInfos.Sum(ii => (ii.Count ?? 0) * (ii.InputPrice ?? 0))
                : 0)
            .Map(dest => dest.Products, src => src.InputInfos);

        config.NewConfig<InputInfo, InputInfoDto>()
            .Map(dest => dest.ProductName, src => src.ProductVariant != null && src.ProductVariant.Product != null
                ? src.ProductVariant.Product.Name
                : null);

        config.NewConfig<UpdateInputRequest, Input>()
            .IgnoreNullValues(true);

        config.NewConfig<UpdateInputInfoRequest, InputInfo>()
            .IgnoreNullValues(true);
    }
}
