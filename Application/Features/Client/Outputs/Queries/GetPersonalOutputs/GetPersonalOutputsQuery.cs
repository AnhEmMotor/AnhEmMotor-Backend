using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;
using System.Collections.Generic;

namespace Application.Features.Client.Outputs.Queries.GetPersonalOutputs;

public class GetPersonalOutputsQuery : IRequest<List<OutputItemResponse>>
{
    public Guid CurrentUserId { get; set; }
    public SieveModel SieveModel { get; set; } = new SieveModel();
}
