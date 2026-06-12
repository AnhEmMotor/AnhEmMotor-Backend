using Application.ApiContracts.Logistics.CarrierSettings.Requests;
using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using MediatR;

namespace Application.Features.Logistics.Commands.TestCarrierConnection;

public class TestCarrierConnectionCommand : IRequest<TestCarrierConnectionResponse>
{
    public int Id { get; set; }
    public TestCarrierConnectionRequest Request { get; set; } = new();
}

