using Application.ApiContracts.Output;
using Domain.Entities;
using Mapster;

namespace Application.Features.Outputs.Mappings;

public sealed class OutputMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Output, OutputResponse>()
            .Map(dest => dest.Total, src => src.OutputInfos != null
                ? src.OutputInfos.Sum(oi => (oi.Count ?? 0) * (oi.Price ?? 0))
                : 0)
            .Map(dest => dest.Products, src => src.OutputInfos);

        config.NewConfig<OutputInfo, OutputInfoDto>()
            .Map(dest => dest.ProductName, src => src.ProductVariant != null && src.ProductVariant.Product != null
                ? src.ProductVariant.Product.Name
                : null);

        config.NewConfig<UpdateOutputRequest, Output>()
            .IgnoreNullValues(true);

        config.NewConfig<UpdateOutputInfoRequest, OutputInfo>()
            .IgnoreNullValues(true);

        config.NewConfig<UpdateOutputRequest, Output>().IgnoreNullValues(true);
    }
}
