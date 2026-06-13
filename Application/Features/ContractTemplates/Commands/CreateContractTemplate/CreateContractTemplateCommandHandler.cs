using Application.Common.Models;
using Application.Interfaces.Repositories.ContractTemplate;
using Domain.Entities;
using MediatR;

namespace Application.Features.ContractTemplates.Commands.CreateContractTemplate;

public class CreateContractTemplateCommandHandler(
    IContractTemplateInsertRepository contractTemplateInsertRepository,
    Application.Interfaces.Repositories.IUnitOfWork unitOfWork) : IRequestHandler<CreateContractTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateContractTemplateCommand request, CancellationToken cancellationToken)
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
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Guid>.Success(entity.Id);
    }
}
