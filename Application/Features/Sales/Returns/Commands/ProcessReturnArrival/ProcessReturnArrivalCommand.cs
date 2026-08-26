using Application.Common.Models;
using MediatR;

namespace Application.Features.Sales.Returns.Commands.ProcessReturnArrival;

public class ProcessReturnArrivalCommand : IRequest<Result<int>>
{
    public int OutputId { get; set; }
}
