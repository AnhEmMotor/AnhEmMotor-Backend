using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.DeleteLead
{
    public class DeleteLeadCommandHandler(
        ILeadDeleteRepository leadDeleteRepository,
        ILeadReadRepository leadReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteLeadCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var lead = await leadReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
                if (lead == null)
                {
                    return Result<bool>.Failure("Khách hàng tiềm năng không tồn tại.");
                }
                await leadDeleteRepository.DeleteByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result<bool>.Success(true);
            } catch (Exception ex)
            {
                return Result<bool>.Failure($"Lỗi khi xóa khách hàng tiềm năng: {ex.Message}");
            }
        }
    }
}
