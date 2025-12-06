using Application.ApiContracts.Output;
using Application.ApiContracts.Output.Responses;
using Application.Features.Outputs.Commands.UpdateOutput;
using Domain.Entities;
using Mapster;

namespace Application.Features.Outputs.Mappings;

public sealed class OutputMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Output, OutputResponse>()
            .Map(
                dest => dest.Total,
                src => src.OutputInfos != null ? src.OutputInfos.Sum(oi => (oi.Count ?? 0) * (oi.Price ?? 0)) : 0)
            .Map(dest => dest.Products, src => src.OutputInfos);

        config.NewConfig<OutputInfo, OutputInfoResponse>()
            .Map(
                dest => dest.ProductName,
                src => src.ProductVariant != null && src.ProductVariant.Product != null
                    ? src.ProductVariant.Product.Name
                    : null);

        config.NewConfig<ApiContracts.Output.Requests.UpdateOutputRequest, Output>().IgnoreNullValues(true);

        config.NewConfig<ApiContracts.Output.Requests.UpdateOutputInfoRequest, OutputInfo>().IgnoreNullValues(true);

        config.NewConfig<ApiContracts.Output.Requests.UpdateOutputRequest, Output>().IgnoreNullValues(true);
        config.NewConfig<ApiContracts.Output.Requests.UpdateOutputInfoRequest, OutputInfo>().IgnoreNullValues(true);
        config.NewConfig<UpdateOutputCommand, Output>().IgnoreNullValues(true).Ignore(dest => dest.OutputInfos);
    }
}
