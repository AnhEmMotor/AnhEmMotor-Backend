using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.AssignLead;

public record AssignLeadCommand(int LeadId, Guid? UserId) : IRequest<Result<int>>;

public class AssignLeadCommandHandler(
    ILeadReadRepository leadReadRepository,
    ILeadWriteRepository leadWriteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignLeadCommand, Result<int>>
{
    public async Task<Result<int>> Handle(AssignLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await leadReadRepository.GetByIdAsync(request.LeadId, cancellationToken);
        if (lead == null)
            return Result<int>.Failure("Không tìm thấy khách hàng.");
        lead.AssignedToId = request.UserId;
        await leadWriteRepository.UpdateAsync(lead, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(lead.Id);
    }
}
