using Application.ApiContracts.ContractTemplate.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ContractTemplate;
using MediatR;

namespace Application.Features.ContractTemplates.Queries.GetContractTemplateById;

public class GetContractTemplateByIdQueryHandler(IContractTemplateReadRepository contractTemplateReadRepository) : IRequestHandler<GetContractTemplateByIdQuery, Result<ContractTemplateResponse>>
{
    public async Task<Result<ContractTemplateResponse>> Handle(
        GetContractTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await contractTemplateReadRepository.GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ContractTemplateResponse>.Failure(Error.NotFound("Mẫu hợp đồng không tồn tại."));
        }
        var response = new ContractTemplateResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            Code = entity.Code,
            Version = entity.Version,
            Content = entity.Content,
            DynamicFields = entity.DynamicFields,
            IsActive = entity.IsActive,
            Status = (int)entity.Status,
            ParentId = entity.ParentId,
            IsUsed = entity.IsUsed
        };
        return Result<ContractTemplateResponse>.Success(response);
    }
}
