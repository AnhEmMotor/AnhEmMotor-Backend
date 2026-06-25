using Application.ApiContracts.SalesContracts.Requests;
using Application.ApiContracts.SalesContracts.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.SalesContracts.Mappings;

public class SalesContractMapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SalesContract, SalesContractResponse>().Map(dest => dest.OrderId, src => src.OutputId ?? 0);
        config.NewConfig<CreateSalesContractRequest, SalesContract>()
            .Map(dest => dest.OutputId, src => src.OrderId)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ContractNumber)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.DeletedAt!)
            .Ignore(dest => dest.SignedDate!)
            .Ignore(dest => dest.ScannedFileUrl!);
    }
}
