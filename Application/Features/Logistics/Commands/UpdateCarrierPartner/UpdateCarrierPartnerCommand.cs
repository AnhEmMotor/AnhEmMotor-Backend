using Application.ApiContracts.Logistics.CarrierSettings.Requests;
using MediatR;

namespace Application.Features.Logistics.Commands.UpdateCarrierPartner;

public class UpdateCarrierPartnerCommand : IRequest<bool>
{
    public int Id { get; set; }
    public UpdateCarrierPartnerRequest Request { get; set; } = new();
}

