using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.AssignLead
{
    public class AssignLeadCommandHandler(
        ILeadReadRepository leadReadRepository,
        ILeadWriteRepository leadWriteRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<AssignLeadCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(AssignLeadCommand request, CancellationToken cancellationToken)
        {
            var lead = await leadReadRepository.GetByIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);
            if (lead == null)
                return Result<int>.Failure("Không tìm thấy khách hàng.");
            lead.AssignedToId = request.UserId;
            await leadWriteRepository.UpdateAsync(lead, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(lead.Id);
        }
    }
}
