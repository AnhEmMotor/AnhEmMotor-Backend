using Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Commands.DeleteContractTemplate;

internal sealed class DeleteContractTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteContractTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        DeleteContractTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ContractTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return Result<Unit>.Failure(Error.NotFound("Mẫu hợp đồng không tồn tại."));
        }

        entity.DeletedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
