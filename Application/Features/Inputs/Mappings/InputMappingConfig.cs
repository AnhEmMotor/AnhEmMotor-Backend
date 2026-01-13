using Application.ApiContracts.Input.Responses;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.ApiContracts.Input.Requests;
using Domain.Entities;
using Mapster;

namespace Application.Features.Inputs.Mappings;

public sealed class InputMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateInputCommand, Input>();
        config.NewConfig<CreateInputInfoRequest, InputInfo>();

        config.NewConfig<Input, InputResponse>()
            .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
            .Map(
                dest => dest.TotalPayable,
                src => src.InputInfos != null ? src.InputInfos.Sum(ii => (ii.Count ?? 0) * (ii.InputPrice ?? 0)) : 0)
            .Map(dest => dest.Products, src => src.InputInfos);

        config.NewConfig<InputInfo, InputInfoResponse>()
            .Map(
                dest => dest.ProductName,
                src => src.ProductVariant != null && src.ProductVariant.Product != null
                    ? src.ProductVariant.Product.Name
                    : null);

        config.NewConfig<ApiContracts.Input.Requests.UpdateInputInfoRequest, InputInfo>().IgnoreNullValues(true);

        config.NewConfig<UpdateInputCommand, Input>().IgnoreNullValues(true);
    }
}
