using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Services;

public interface ILeadAssignmentService
{
    Task AssignLeadAsync(Lead lead, CancellationToken cancellationToken = default);
}
