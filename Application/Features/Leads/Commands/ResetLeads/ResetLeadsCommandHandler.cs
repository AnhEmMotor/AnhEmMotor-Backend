using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Leads.Commands.ResetLeads
{
    public class ResetLeadsCommandHandler(ILeadWriteRepository leadWriteRepository, IUnitOfWork unitOfWork) : IRequestHandler<ResetLeadsCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ResetLeadsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await leadWriteRepository.ClearAllAsync(cancellationToken).ConfigureAwait(false);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Lỗi khi reset dữ liệu: {ex.Message}");
            }
        }
    }

}
