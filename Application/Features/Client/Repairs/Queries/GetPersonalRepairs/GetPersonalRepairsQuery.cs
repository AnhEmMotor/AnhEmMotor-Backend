using Application.ApiContracts.Admin.Workshop.Responses;
using MediatR;
using Sieve.Models;
using System.Collections.Generic;

namespace Application.Features.Client.Repairs.Queries.GetPersonalRepairs;

public class GetPersonalRepairsQuery : IRequest<List<RepairOrderResponse>>
{
    public Guid CurrentUserId { get; set; }
    public SieveModel SieveModel { get; set; } = new SieveModel();
}
