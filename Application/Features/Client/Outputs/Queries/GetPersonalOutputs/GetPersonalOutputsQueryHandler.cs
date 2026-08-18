using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Sieve.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Client.Outputs.Queries.GetPersonalOutputs;

public class GetPersonalOutputsQueryHandler : IRequestHandler<GetPersonalOutputsQuery, List<OutputItemResponse>>
{
    private readonly IOutputReadRepository _outputReadRepository;

    public GetPersonalOutputsQueryHandler(IOutputReadRepository outputReadRepository)
    {
        _outputReadRepository = outputReadRepository;
    }

    public async Task<List<OutputItemResponse>> Handle(GetPersonalOutputsQuery request, CancellationToken cancellationToken)
    {
        var sieveModel = request.SieveModel ?? new SieveModel();
        if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
        {
            sieveModel.Sorts = "-CreatedAt";
        }
        
        var outputs = await _outputReadRepository.GetPagedAsync<OutputItemResponse>(
            sieveModel,
            DataFetchMode.ActiveOnly,
            o => o.BuyerId == request.CurrentUserId && 
                 !o.OutputInfos.Any(oi => oi.ProductVariant != null && 
                                          oi.ProductVariant.Product != null && 
                                          oi.ProductVariant.Product.ProductCategory != null && 
                                          oi.ProductVariant.Product.ProductCategory.ManagementType == "vin_number"),
            false,
            cancellationToken);

        return outputs.Items?.ToList() ?? new List<OutputItemResponse>();
    }
}
