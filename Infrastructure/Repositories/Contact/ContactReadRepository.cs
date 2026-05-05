using Application.Interfaces.Repositories.Contact;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Contact;

public class ContactReadRepository(ApplicationDBContext context) : IContactReadRepository
{
    public Task<Domain.Entities.Contact?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Contacts
            .Include(c => c.Replies)
            .ThenInclude(r => r.RepliedBy)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<List<Domain.Entities.Contact>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.Contacts.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }
}
