using Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Commands.CloneContractTemplate;

internal sealed class CloneContractTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CloneContractTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CloneContractTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var original = await context.ContractTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (original is null)
        {
            return Result<Guid>.Failure(Error.NotFound("Mẫu hợp đồng không tồn tại."));
        }

        original.IsActive = false;

        var clone = new ContractTemplate
        {
            Id = Guid.NewGuid(),
            Name = original.Name,
            Type = original.Type,
            Code = original.Code,
            Version = original.Version + 0.1m,
            Content = original.Content,
            DynamicFields = original.DynamicFields,
            IsActive = true
        };

        await context.ContractTemplates.AddAsync(clone, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(clone.Id);
    }
}
