using Application.ApiContracts.Admin.Warranty;
using Application.Features.WarrantyTerms.Commands.CreateWarrantyTerm;
using Domain.Entities;
using Mapster;

namespace Application.Features.WarrantyTerms.Mappings;

public class WarrantyTermMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateWarrantyTermCommand, WarrantyTerm>();
        config.NewConfig<WarrantyTerm, WarrantyTermResponse>();
    }
}
