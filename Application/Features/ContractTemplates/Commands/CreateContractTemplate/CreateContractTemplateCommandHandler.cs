using Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Commands.CreateContractTemplate;

internal sealed class CreateContractTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateContractTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateContractTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new ContractTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Request.Name,
            Type = request.Request.Type,
            Code = request.Request.Code,
            Version = 1.0m,
            Content = request.Request.Content,
            DynamicFields = request.Request.DynamicFields,
            IsActive = true
        };

        await context.ContractTemplates.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
