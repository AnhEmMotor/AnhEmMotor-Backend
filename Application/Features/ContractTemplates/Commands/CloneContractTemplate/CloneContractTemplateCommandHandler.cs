using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ContractTemplate;
using Domain.Entities;
using MediatR;

namespace Application.Features.ContractTemplates.Commands.CloneContractTemplate;

public class CloneContractTemplateCommandHandler(
    IContractTemplateReadRepository contractTemplateReadRepository,
    IContractTemplateInsertRepository contractTemplateInsertRepository,
    IContractTemplateUpdateRepository contractTemplateUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CloneContractTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CloneContractTemplateCommand request, CancellationToken cancellationToken)
    {
        var original = await contractTemplateReadRepository.GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
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
        contractTemplateUpdateRepository.Update(original);
        contractTemplateInsertRepository.Add(clone);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Guid>.Success(clone.Id);
    }
}
