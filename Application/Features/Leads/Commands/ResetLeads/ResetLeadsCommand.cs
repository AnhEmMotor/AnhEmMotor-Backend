using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.ResetLeads;

public record ResetLeadsCommand : IRequest<Result<bool>>;

public class ResetLeadsCommandHandler(ILeadWriteRepository leadWriteRepository, IUnitOfWork unitOfWork) : IRequestHandler<ResetLeadsCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ResetLeadsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await leadWriteRepository.ClearAllAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        } catch (Exception ex)
        {
            return Result<bool>.Failure($"Lỗi khi reset dữ liệu: {ex.Message}");
        }
    }
}
