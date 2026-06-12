using Application.Common.Models;
using Application.Interfaces.Repositories.ContractTemplate;
using Domain.Primitives;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Commands.DeleteContractTemplate;

internal sealed class DeleteContractTemplateCommandHandler(
    IContractTemplateReadRepository contractTemplateReadRepository,
    IContractTemplateUpdateRepository contractTemplateUpdateRepository,
    Application.Interfaces.Repositories.IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteContractTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        DeleteContractTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await contractTemplateReadRepository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result<Unit>.Failure(Error.NotFound("Mẫu hợp đồng không tồn tại."));
        }

        entity.DeletedAt = DateTimeOffset.UtcNow;

        contractTemplateUpdateRepository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
