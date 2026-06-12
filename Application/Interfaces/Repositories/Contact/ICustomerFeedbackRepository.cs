using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface ICustomerFeedbackRepository
{
    Task<CustomerFeedback?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<CustomerFeedback>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(CustomerFeedback entity, CancellationToken cancellationToken);
    Task UpdateAsync(CustomerFeedback entity, CancellationToken cancellationToken);
}
