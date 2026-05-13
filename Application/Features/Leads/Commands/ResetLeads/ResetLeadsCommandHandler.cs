using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.ResetLeads
{
    public class ResetLeadsCommandHandler(ILeadDeleteRepository leadDeleteRepository, IUnitOfWork unitOfWork) : IRequestHandler<ResetLeadsCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ResetLeadsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await leadDeleteRepository.ClearAllAsync(cancellationToken).ConfigureAwait(false);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result<bool>.Success(true);
            } catch (Exception ex)
            {
                return Result<bool>.Failure($"Lỗi khi reset dữ liệu: {ex.Message}");
            }
        }
    }
}
