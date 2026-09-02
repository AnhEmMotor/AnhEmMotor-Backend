using Application.ApiContracts.SalesContracts.Requests;
using Application.ApiContracts.SalesContracts.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.SalesContracts.Mappings;

public class SalesContractMapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SalesContract, SalesContractResponse>()
            .Map(dest => dest.InvoiceNumber, src => src.Invoice.InvoiceNumber);

        config.NewConfig<CreateSalesContractRequest, SalesContract>()
            .Map(dest => dest.InvoiceId, src => src.InvoiceId)
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
