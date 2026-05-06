using Domain.Entities;

namespace Application.Interfaces.Services;

public interface ILeadAssignmentService
{
    Task AssignLeadAsync(Lead lead, CancellationToken cancellationToken = default);
}
