using Domain.Entities;

namespace Application.Interfaces.Services;

public interface ILeadAssignmentService
{
    public Task AssignLeadAsync(Lead lead, CancellationToken cancellationToken = default);
}
