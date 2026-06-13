using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface ICustomerFeedbackRepository
{
    public Task<CustomerFeedback?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public Task<List<CustomerFeedback>> GetAllAsync(CancellationToken cancellationToken);

    public Task AddAsync(CustomerFeedback entity, CancellationToken cancellationToken);

    public Task UpdateAsync(CustomerFeedback entity, CancellationToken cancellationToken);
}
