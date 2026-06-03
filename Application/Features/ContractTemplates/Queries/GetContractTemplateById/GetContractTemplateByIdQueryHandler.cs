using Application.Common.Models;
using Application.ApiContracts.ContractTemplate.Responses;
using Application.Common.Interfaces;
using Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Queries.GetContractTemplateById;

internal sealed class GetContractTemplateByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetContractTemplateByIdQuery, Result<ContractTemplateResponse>>
{
    public async Task<Result<ContractTemplateResponse>> Handle(
        GetContractTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ContractTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

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
