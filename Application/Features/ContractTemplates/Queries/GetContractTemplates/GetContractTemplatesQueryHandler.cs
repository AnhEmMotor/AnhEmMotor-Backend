using Application.ApiContracts.ContractTemplate.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ContractTemplate;
using Domain.Primitives;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Queries.GetContractTemplates;

public class GetContractTemplatesQueryHandler(IContractTemplateReadRepository contractTemplateReadRepository)
: IRequestHandler<GetContractTemplatesQuery, Result<PagedResult<ContractTemplateResponse>>>
{
    public async Task<Result<PagedResult<ContractTemplateResponse>>> Handle(
        GetContractTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var items = await contractTemplateReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var query = items.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
        }

        var totalCount = query.Count();

        var pageItems = query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ContractTemplateResponse
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Code = x.Code,
                Version = x.Version,
                Content = x.Content,
                DynamicFields = x.DynamicFields,
                IsActive = x.IsActive,
                Status = (int)x.Status,
                ParentId = x.ParentId,
                IsUsed = x.IsUsed
            })
            .ToList();

        var result = new PagedResult<ContractTemplateResponse>(pageItems, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<ContractTemplateResponse>>.Success(result);
    }
}
