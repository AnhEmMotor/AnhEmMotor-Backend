using Application.Common.Models;
using Application.Interfaces.Repositories.ContractTemplate;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Commands.CreateContractTemplate;

internal sealed class CreateContractTemplateCommandHandler(IContractTemplateInsertRepository contractTemplateInsertRepository, Application.Interfaces.Repositories.IUnitOfWork unitOfWork)
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

        contractTemplateInsertRepository.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
