using Application.ApiContracts.Output.Responses;
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
            .Map(dest => dest.BuyerName, src => src.Buyer != null ? src.Buyer.FullName : null)
            .Map(
                dest => dest.CompletedByUserName,
                src => src.CompletedByUser != null ? src.CompletedByUser.FullName : null)
            .Map(dest => dest.CreatedByUserId, src => src.CreatedByUserId)
            .Map(dest => dest.Products, src => src.OutputInfos);

        config.NewConfig<OutputInfo, OutputInfoResponse>()
            .Map(
                dest => dest.ProductName,
                src => src.ProductVariant != null && src.ProductVariant.Product != null
                    ? src.ProductVariant.Product.Name
                    : null);

        config.NewConfig<ApiContracts.Output.Requests.UpdateOutputForManagerRequest, Output>().IgnoreNullValues(true);

        config.NewConfig<ApiContracts.Output.Requests.UpdateOutputInfoRequest, OutputInfo>().IgnoreNullValues(true);

        config.NewConfig<ApiContracts.Output.Requests.UpdateOutputForManagerRequest, Output>().IgnoreNullValues(true);
        config.NewConfig<ApiContracts.Output.Requests.UpdateOutputInfoRequest, OutputInfo>().IgnoreNullValues(true);
        config.NewConfig<Commands.UpdateOutputForManager.UpdateOutputForManagerCommand, Output>()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.OutputInfos);
    }
}
